using AutoMapper;
using CoachBoard.Application.DTOs;
using CoachBoard.Application.Interfaces;
using CoachBoard.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoachBoard.Api.Controllers;

[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Tags("ClientProgress")]
[Route("api/clients/{clientId:int}/progress")]
[Authorize(Roles = "Coach,Admin")]
public class ClientProgressController : ControllerBase
{
    private readonly IClientRepository _clients;
    private readonly IClientProgressRepository _progress;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public ClientProgressController(
        IClientRepository clients,
        IClientProgressRepository progress,
        ICurrentUserService currentUser,
        IMapper mapper)
    {
        _clients = clients;
        _progress = progress;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClientProgressReadDto>>> GetByClient(int clientId)
    {
        var client = await _clients.GetByIdAsync(clientId);
        if (client is null) return NotFound();
        if (!CurrentCoachCanAccessClient(client)) return Forbid();

        var records = await _progress.GetByClientAsync(clientId);
        return Ok(_mapper.Map<IEnumerable<ClientProgressReadDto>>(records));
    }

    [HttpGet("{progressId:int}")]
    public async Task<ActionResult<ClientProgressReadDto>> GetById(int clientId, int progressId)
    {
        var client = await _clients.GetByIdAsync(clientId);
        if (client is null) return NotFound();
        if (!CurrentCoachCanAccessClient(client)) return Forbid();

        var record = await _progress.GetByClientAndIdAsync(clientId, progressId);
        if (record is null) return NotFound();

        return Ok(_mapper.Map<ClientProgressReadDto>(record));
    }

    [HttpPost]
    public async Task<ActionResult<ClientProgressReadDto>> Create(int clientId, [FromBody] ClientProgressCreateDto input)
    {
        var client = await _clients.GetByIdAsync(clientId);
        if (client is null) return NotFound();
        if (!CurrentCoachCanAccessClient(client)) return Forbid();

        var entity = _mapper.Map<ClientProgressRecord>(input);
        entity.ClientId = clientId;

        await _progress.AddAsync(entity);
        await _progress.SaveChangesAsync();

        var read = _mapper.Map<ClientProgressReadDto>(entity);
        return CreatedAtAction(nameof(GetById), new { clientId, progressId = entity.Id }, read);
    }

    [HttpPut("{progressId:int}")]
    public async Task<ActionResult<ClientProgressReadDto>> Update(int clientId, int progressId, [FromBody] ClientProgressUpdateDto input)
    {
        var client = await _clients.GetByIdAsync(clientId);
        if (client is null) return NotFound();
        if (!CurrentCoachCanAccessClient(client)) return Forbid();

        var entity = await _progress.GetByClientAndIdAsync(clientId, progressId);
        if (entity is null) return NotFound();

        _mapper.Map(input, entity);

        await _progress.UpdateAsync(entity);
        await _progress.SaveChangesAsync();

        return Ok(_mapper.Map<ClientProgressReadDto>(entity));
    }

    [HttpDelete("{progressId:int}")]
    public async Task<IActionResult> Delete(int clientId, int progressId)
    {
        var client = await _clients.GetByIdAsync(clientId);
        if (client is null) return NotFound();
        if (!CurrentCoachCanAccessClient(client)) return Forbid();

        var entity = await _progress.GetByClientAndIdAsync(clientId, progressId);
        if (entity is null) return NotFound();

        await _progress.DeleteAsync(entity);
        await _progress.SaveChangesAsync();

        return NoContent();
    }

    private bool CurrentCoachCanAccessClient(Client client)
    {
        if (!_currentUser.IsCoach) return true;

        return _currentUser.CoachId is not null && client.CoachId == _currentUser.CoachId.Value;
    }
}
