using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using CoachBoard.Application.Interfaces;
using CoachBoard.Application.DTOs;
using CoachBoard.Domain.Entities;
using CoachBoard.Api.Filters;

namespace CoachBoard.Api.Controllers;

[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Tags("Routine")]
[Route("api/[controller]")]
public class RoutinesController : ControllerBase
{
    private readonly IRoutineRepository _repo;
    private readonly IClientRepository _clientRepo;
    private readonly IExerciseRepository _exerciseRepo;
    private readonly IMapper _mapper;
    private readonly IPlanLimitsProvider _limits;
    private readonly ITenantRepository _tenantRepo;
    private readonly ICurrentTenant _currentTenant;
    private readonly IFeatureFlags _featureFlags;
    private readonly ICurrentUserService _currentUser;

    public RoutinesController(IRoutineRepository repo, IClientRepository clientRepo, IExerciseRepository exerciseRepo, IMapper mapper, IPlanLimitsProvider limits, ITenantRepository tenantRepo, ICurrentTenant currentTenant, IFeatureFlags featureFlags, ICurrentUserService currentUser)
    {
        _repo = repo; _clientRepo = clientRepo; _exerciseRepo = exerciseRepo; _mapper = mapper;
        _limits = limits;
        _tenantRepo = tenantRepo;
        _currentTenant = currentTenant;
        _featureFlags = featureFlags;
        _currentUser = currentUser;
    }

    // GET /api/routines?clientId=1&page=1&pageSize=20&q=pecho
    [HttpGet]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<ActionResult<PagedResult<RoutineReadDto>>> Get([FromQuery] int clientId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? q = null)
    {
        if (clientId <= 0) return BadRequest("clientId es requerido y > 0");

        if (!await CurrentCoachCanAccessClientAsync(clientId)) return Forbid();

        var items = await _repo.GetByClientAsync(clientId, page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, q);
        var total = await _repo.CountByClientAsync(clientId, q);
        var dto = _mapper.Map<IEnumerable<RoutineReadDto>>(items);
        return Ok(new PagedResult<RoutineReadDto>(dto, total, page, pageSize));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<ActionResult<RoutineReadDto>> GetById(int id)
    {
        var routine = await _repo.GetWithItemsAsync(id);
        if (routine is null) return NotFound();

        if (!await CurrentCoachCanAccessClientAsync(routine.ClientId)) return Forbid();

        return Ok(_mapper.Map<RoutineReadDto>(routine));
    }

    [HttpGet("{id:int}/export")]
    [Authorize(Roles = "Coach,Admin")]
    [RequirePro]
    public async Task<IActionResult> Export(int id)
    {
        if (!await _featureFlags.IsEnabledAsync("feature.export_routine"))
        {
            return StatusCode(403, "Feature 'feature.export_routine' is not enabled for this tenant.");
        }

        var routine = await _repo.GetWithItemsAsync(id);
        if (routine is null) return NotFound();

        if (!await CurrentCoachCanAccessClientAsync(routine.ClientId)) return Forbid();
        
        // Simulating export content
        var content = $"Export for Routine {routine.Title} (Client: {routine.Client?.FullName ?? "Unknown"})";
        return File(System.Text.Encoding.UTF8.GetBytes(content), "text/plain", $"routine_{id}.txt");
    }

    [HttpPost]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<ActionResult<RoutineReadDto>> Create([FromBody] RoutineCreateDto input)
    {
        // validar client
        var client = await _clientRepo.GetByIdAsync(input.ClientId);
        if (client is null) return BadRequest("ClientId inválido.");

        if (!CurrentCoachCanAccessClient(client)) return Forbid();

        // Check Plan Limits
        var tenantId = _currentTenant.TenantId ?? 0;
        var tenant = await _tenantRepo.GetByIdAsync(tenantId);

        if (tenant is not null)
        {
            var limits = _limits.GetLimits(tenant.Plan);
            if (limits.MaxRoutines != -1)
            {
                var count = await _repo.CountAsync();
                if (count >= limits.MaxRoutines)
                {
                    return Conflict($"Has alcanzado el límite de {limits.MaxRoutines} rutinas para tu plan {tenant.Plan}.");
                }
            }
        }

        // validar exercises existen
        var exerciseIds = input.Items.Select(i => i.ExerciseId).Distinct().ToArray();
        foreach (var exId in exerciseIds)
        {
            if (await _exerciseRepo.GetByIdAsync(exId) is null)
                return BadRequest($"ExerciseId inválido: {exId}");
        }

        var routine = _mapper.Map<Routine>(input);
        await _repo.AddAsync(routine);
        await _repo.SaveChangesAsync(); // para obtener RoutineId

        // mapear items con el routineId recién creado
        var items = input.Items.Select(i => new RoutineExercise
        {
            RoutineId = routine.Id,
            ExerciseId = i.ExerciseId,
            Sets = i.Sets,
            Reps = i.Reps,
            Order = i.Order,
            Notes = i.Notes
        }).ToList();

        await _repo.ReplaceItemsAsync(routine.Id, items);
        await _repo.SaveChangesAsync();

        var full = await _repo.GetWithItemsAsync(routine.Id)!;
        return CreatedAtAction(nameof(GetById), new { id = routine.Id }, _mapper.Map<RoutineReadDto>(full));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<ActionResult<RoutineReadDto>> Update(int id, [FromBody] RoutineUpdateDto input)
    {
        var routine = await _repo.GetByIdAsync(id);
        if (routine is null) return NotFound();

        if (!await CurrentCoachCanAccessClientAsync(routine.ClientId)) return Forbid();

        _mapper.Map(input, routine);
        await _repo.UpdateAsync(routine);

        // validar exercises
        var exerciseIds = input.Items.Select(i => i.ExerciseId).Distinct().ToArray();
        foreach (var exId in exerciseIds)
        {
            if (await _exerciseRepo.GetByIdAsync(exId) is null)
                return BadRequest($"ExerciseId inválido: {exId}");
        }

        var items = input.Items.Select(i => new RoutineExercise
        {
            RoutineId = routine.Id,
            ExerciseId = i.ExerciseId,
            Sets = i.Sets,
            Reps = i.Reps,
            Order = i.Order,
            Notes = i.Notes
        }).ToList();

        await _repo.ReplaceItemsAsync(routine.Id, items);
        await _repo.SaveChangesAsync();

        var full = await _repo.GetWithItemsAsync(routine.Id)!;
        return Ok(_mapper.Map<RoutineReadDto>(full));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var routine = await _repo.GetByIdAsync(id);
        if (routine is null) return NotFound();

        if (!await CurrentCoachCanAccessClientAsync(routine.ClientId)) return Forbid();

        await _repo.DeleteAsync(routine);
        await _repo.SaveChangesAsync();
        return NoContent();
    }

    private async Task<bool> CurrentCoachCanAccessClientAsync(int clientId)
    {
        if (!_currentUser.IsCoach) return true;

        var client = await _clientRepo.GetByIdAsync(clientId);
        if (client is null) return false;

        return CurrentCoachCanAccessClient(client);
    }

    private bool CurrentCoachCanAccessClient(Client client)
    {
        if (!_currentUser.IsCoach) return true;

        return _currentUser.CoachId is not null && client.CoachId == _currentUser.CoachId.Value;
    }
}
