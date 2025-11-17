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
    [HttpGet]
    [Authorize(Roles = "Coach,Admin")]
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


    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ClientReadDto>> GetById(int id)
    {
        var client = await _repo.GetByIdAsync(id);
        return client is null ? NotFound() : Ok(_mapper.Map<ClientReadDto>(client));
    }

    [HttpPost]
    [Authorize(Roles = "Coach,Admin")]
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


    [HttpPut("{id:int}")]
    [Authorize(Roles = "Coach,Admin")]
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

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
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

