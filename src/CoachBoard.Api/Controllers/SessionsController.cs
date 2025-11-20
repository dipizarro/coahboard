using AutoMapper;
using CoachBoard.Application.DTOs;
using CoachBoard.Application.Interfaces;
using CoachBoard.Application.Validators;
using CoachBoard.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoachBoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionsController : ControllerBase
{
    private readonly ISessionRepository _sessions;
    private readonly ICoachRepository _coaches;
    private readonly IClientRepository _clients;
    private readonly IRoutineRepository _routines;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public SessionsController(
        ISessionRepository sessions,
        ICoachRepository coaches,
        IClientRepository clients,
        IRoutineRepository routines,
        ICurrentUserService currentUser,
        IMapper mapper)
    {
        _sessions = sessions;
        _coaches = coaches;
        _clients = clients;
        _routines = routines;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    // GET /api/sessions?coachId=1&from=2025-03-01&to=2025-03-31&clientId=5
    [HttpGet]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<ActionResult<IEnumerable<SessionReadDto>>> Get(
        [FromQuery] int? coachId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? clientId)
    {
        int effectiveCoachId;

        if (_currentUser.IsCoach)
        {
            if (_currentUser.CoachId is null) return Forbid();
            effectiveCoachId = _currentUser.CoachId.Value;

            if (coachId.HasValue && coachId.Value != effectiveCoachId)
                return Forbid(); // coach intentando ver sesiones de otro coach
        }
        else // Admin
        {
            if (!coachId.HasValue || coachId.Value <= 0)
                return BadRequest("coachId es requerido para Admin.");
            effectiveCoachId = coachId.Value;
        }

        var fromDate = from ?? DateTime.UtcNow.Date;
        var toDate = to ?? fromDate.AddDays(30);

        var sessions = await _sessions.GetByCoachAsync(effectiveCoachId, fromDate, toDate, clientId);
        var dto = _mapper.Map<IEnumerable<SessionReadDto>>(sessions);
        return Ok(dto);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<ActionResult<SessionReadDto>> GetById(int id)
    {
        var session = await _sessions.GetWithRelationsAsync(id);
        if (session is null) return NotFound();

        if (_currentUser.IsCoach)
        {
            if (_currentUser.CoachId is null || session.CoachId != _currentUser.CoachId.Value)
                return Forbid();
        }

        return Ok(_mapper.Map<SessionReadDto>(session));
    }

    [HttpPost]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<ActionResult<SessionReadDto>> Create([FromBody] SessionCreateDto input)
    {
        int effectiveCoachId;

        if (_currentUser.IsCoach)
        {
            if (_currentUser.CoachId is null) return Forbid();
            effectiveCoachId = _currentUser.CoachId.Value;
        }
        else // Admin
        {
            if (!input.CoachId.HasValue || input.CoachId.Value <= 0)
                return BadRequest("CoachId es requerido.");
            effectiveCoachId = input.CoachId.Value;
        }

        // Validar coach
        var coach = await _coaches.GetByIdAsync(effectiveCoachId);
        if (coach is null) return BadRequest("CoachId inválido.");

        // Validar client si viene
        if (input.ClientId.HasValue)
        {
            var client = await _clients.GetByIdAsync(input.ClientId.Value);
            if (client is null || client.CoachId != effectiveCoachId)
                return BadRequest("ClientId inválido para este coach.");
        }

        // Validar routine si viene (opcional: podrías chequear que la rutina sea del client)
        if (input.RoutineId.HasValue)
        {
            var routine = await _routines.GetByIdAsync(input.RoutineId.Value);
            if (routine is null)
                return BadRequest("RoutineId inválido.");
        }

        var entity = _mapper.Map<Session>(input);
        entity.CoachId = effectiveCoachId;

        await _sessions.AddAsync(entity);
        await _sessions.SaveChangesAsync();

        var full = await _sessions.GetWithRelationsAsync(entity.Id) ?? entity;
        var read = _mapper.Map<SessionReadDto>(full);

        return CreatedAtAction(nameof(GetById), new { id = read.Id }, read);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<ActionResult<SessionReadDto>> Update(int id, [FromBody] SessionUpdateDto input)
    {
        var entity = await _sessions.GetByIdAsync(id);
        if (entity is null) return NotFound();

        if (_currentUser.IsCoach)
        {
            if (_currentUser.CoachId is null || entity.CoachId != _currentUser.CoachId.Value)
                return Forbid();
        }

        // Validar client si viene
        if (input.ClientId.HasValue)
        {
            var client = await _clients.GetByIdAsync(input.ClientId.Value);
            if (client is null || client.CoachId != entity.CoachId)
                return BadRequest("ClientId inválido para este coach.");
        }

        if (input.RoutineId.HasValue)
        {
            var routine = await _routines.GetByIdAsync(input.RoutineId.Value);
            if (routine is null)
                return BadRequest("RoutineId inválido.");
        }

        _mapper.Map(input, entity);

        await _sessions.UpdateAsync(entity);
        await _sessions.SaveChangesAsync();

        var full = await _sessions.GetWithRelationsAsync(entity.Id) ?? entity;
        var read = _mapper.Map<SessionReadDto>(full);
        return Ok(read);
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] SessionStatusUpdateDto input)
    {
        var entity = await _sessions.GetByIdAsync(id);
        if (entity is null) return NotFound();

        if (_currentUser.IsCoach)
        {
            if (_currentUser.CoachId is null || entity.CoachId != _currentUser.CoachId.Value)
                return Forbid();
        }

        // Validación simple; el validator de FluentValidation también aplicará
        if (!SessionValidationConstants.AllowedStatuses.Contains(input.Status))
            return BadRequest($"Status inválido. Valores permitidos: {string.Join(", ", SessionValidationConstants.AllowedStatuses)}.");

        entity.Status = input.Status;

        await _sessions.UpdateAsync(entity);
        await _sessions.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _sessions.GetByIdAsync(id);
        if (entity is null) return NotFound();

        if (_currentUser.IsCoach)
        {
            if (_currentUser.CoachId is null || entity.CoachId != _currentUser.CoachId.Value)
                return Forbid();
        }

        await _sessions.DeleteAsync(entity);
        await _sessions.SaveChangesAsync();
        return NoContent();
    }
}
