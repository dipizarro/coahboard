using AutoMapper;
using CoachBoard.Application.DTOs;
using CoachBoard.Application.Interfaces;
using CoachBoard.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CoachBoard.Api.Controllers;

[ApiController]
[ApiExplorerSettings(GroupName ="v1")]
[Tags("Clients")]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly IClientRepository _repo;
    private readonly IMapper _mapper;
    private readonly ICoachRepository _coachRepo;

    public ClientsController(IClientRepository repo, ICoachRepository coachRepo, IMapper mapper)
    {
        _repo = repo;
        _coachRepo = coachRepo;
        _mapper = mapper;
    }

    // GET /api/clients?coachId=1&page=1&pageSize=20&q=ana
    /// <summary>
    /// Obtiene una lista paginada de clientes asociados a un Coach.
    /// </summary>
    /// <param name="coachId">ID del Coach.</param>
    /// <param name="page">Número de página (default 1).</param>
    /// <param name="pageSize">Tamaño de página (default 20).</param>
    /// <param name="q">Término de búsqueda opcional (nombre/email).</param>
    /// <returns>Lista paginada de clientes.</returns>
    [HttpGet]
    [Authorize(Roles = "Coach,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<ClientReadDto>>> Get(
    [FromQuery] int coachId,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] string? q = null)
    {
        var role = GetRole();

        if (role == "Coach")
        {
            var coachIdFromToken = GetCoachIdFromToken();
            if (coachIdFromToken is null) return Forbid();

            // Si el cliente mandó otro coachId que no es el suyo → FORBIDDEN
            if (coachId != 0 && coachId != coachIdFromToken.Value)
                return Forbid();

            coachId = coachIdFromToken.Value;
        }

        if (coachId <= 0) return BadRequest("coachId es requerido y debe ser > 0.");

        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 20 : pageSize;

        var items = await _repo.GetByCoachAsync(coachId, page, pageSize, q);
        var total = await _repo.CountByCoachAsync(coachId, q);

        var dto = _mapper.Map<IEnumerable<ClientReadDto>>(items);
        return Ok(new PagedResult<ClientReadDto>(dto, total, page, pageSize));
    }


    /// <summary>
    /// Obtiene un cliente por su ID.
    /// </summary>
    /// <param name="id">ID del cliente.</param>
    /// <returns>Detalles del cliente.</returns>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientReadDto>> GetById(int id)
    {
        var client = await _repo.GetByIdAsync(id);
        return client is null ? NotFound() : Ok(_mapper.Map<ClientReadDto>(client));
    }

    /// <summary>
    /// Crea un nuevo cliente.
    /// </summary>
    /// <param name="input">Datos del nuevo cliente.</param>
    /// <returns>El cliente creado.</returns>
    [HttpPost]
    [Authorize(Roles = "Coach,Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ClientReadDto>> Create([FromBody] ClientCreateDto input)
    {
        var role = GetRole();

        if (role == "Coach")
        {
            var coachIdFromToken = GetCoachIdFromToken();
            if (coachIdFromToken is null) return Forbid();

            if (input.CoachId != coachIdFromToken.Value)
                return Forbid(); // intenta crear cliente para otro coach
        }

        var coach = await _coachRepo.GetByIdAsync(input.CoachId);
        if (coach is null) return BadRequest("CoachId inválido.");

        var entity = _mapper.Map<Client>(input);
        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();

        var read = _mapper.Map<ClientReadDto>(entity);
        return CreatedAtAction(nameof(GetById), new { id = read.Id }, read);
    }


    /// <summary>
    /// Actualiza un cliente existente.
    /// </summary>
    /// <param name="id">ID del cliente a actualizar.</param>
    /// <param name="input">Datos actualizados.</param>
    /// <returns>El cliente actualizado.</returns>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Coach,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientReadDto>> Update(int id, [FromBody] ClientUpdateDto input)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null) return NotFound();

        var role = GetRole();
        if (role == "Coach")
        {
            var coachIdFromToken = GetCoachIdFromToken();
            if (coachIdFromToken is null) return Forbid();
            if (entity.CoachId != coachIdFromToken.Value) return Forbid();
        }

        _mapper.Map(input, entity);
        await _repo.UpdateAsync(entity);
        await _repo.SaveChangesAsync();

        return Ok(_mapper.Map<ClientReadDto>(entity));
    }

    /// <summary>
    /// Elimina un cliente.
    /// </summary>
    /// <param name="id">ID del cliente a eliminar.</param>
    /// <returns>No Content si fue eliminado.</returns>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Coach,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null) return NotFound();

        var role = GetRole();
        if (role == "Coach")
        {
            var coachIdFromToken = GetCoachIdFromToken();
            if (coachIdFromToken is null) return Forbid();
            if (entity.CoachId != coachIdFromToken.Value) return Forbid();
        }

        await _repo.DeleteAsync(entity);
        await _repo.SaveChangesAsync();
        return NoContent();
    }

    private string? GetRole() =>
    User.FindFirst(ClaimTypes.Role)?.Value;

    private int? GetCoachIdFromToken()
    {
        var claim = User.FindFirst("coachId");
        if (claim is null) return null;
        return int.TryParse(claim.Value, out var id) ? id : null;
    }

}

