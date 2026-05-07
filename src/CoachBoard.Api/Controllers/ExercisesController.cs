using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using CoachBoard.Application.Interfaces;
using CoachBoard.Application.DTOs;
using CoachBoard.Domain.Entities;

namespace CoachBoard.Api.Controllers;

[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Tags("Exercises")]
[Route("api/[controller]")]
public class ExercisesController : ControllerBase
{
    private readonly IExerciseRepository _repo;
    private readonly ICoachRepository _coaches;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public ExercisesController(
        IExerciseRepository repo,
        ICoachRepository coaches,
        ICurrentUserService currentUser,
        IMapper mapper)
    {
        _repo = repo;
        _coaches = coaches;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    // GET /api/exercises?q=press&category=fuerza&page=1&pageSize=20
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<ExerciseReadDto>>> Search(
        [FromQuery] string? q,
        [FromQuery] string? category,
        [FromQuery] string? targetMuscleGroup,
        [FromQuery] string? equipment,
        [FromQuery] string? difficultyLevel,
        [FromQuery] string? exerciseType,
        [FromQuery] string? environment,
        [FromQuery] string? tag,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 20 : pageSize;

        var items = await _repo.SearchAsync(
            q,
            category,
            targetMuscleGroup,
            equipment,
            difficultyLevel,
            exerciseType,
            environment,
            tag,
            page,
            pageSize,
            _currentUser.IsAdmin,
            CurrentCoachIdOrNull());
        var total = await _repo.CountAsync(
            q,
            category,
            targetMuscleGroup,
            equipment,
            difficultyLevel,
            exerciseType,
            environment,
            tag,
            _currentUser.IsAdmin,
            CurrentCoachIdOrNull());
        var dto = _mapper.Map<IEnumerable<ExerciseReadDto>>(items);

        return Ok(new PagedResult<ExerciseReadDto>(dto, total, page, pageSize));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ExerciseReadDto>> GetById(int id)
    {
        var ex = await _repo.GetByIdAsync(id);
        if (ex is null) return NotFound();
        if (!CanView(ex)) return NotFound();

        return Ok(_mapper.Map<ExerciseReadDto>(ex));
    }

    [HttpPost]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<ActionResult<ExerciseReadDto>> Create([FromBody] ExerciseCreateDto input)
    {
        var entity = _mapper.Map<Exercise>(input);
        var ownershipResult = await ApplyCreateOwnershipAsync(entity, input);
        if (ownershipResult is not null) return ownershipResult;

        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, _mapper.Map<ExerciseReadDto>(entity));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<ActionResult<ExerciseReadDto>> Update(int id, [FromBody] ExerciseUpdateDto input)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null) return NotFound();
        if (!CanEdit(entity)) return Forbid();

        _mapper.Map(input, entity);
        var ownershipResult = await ApplyUpdateOwnershipAsync(entity, input);
        if (ownershipResult is not null) return ownershipResult;

        await _repo.UpdateAsync(entity);
        await _repo.SaveChangesAsync();
        return Ok(_mapper.Map<ExerciseReadDto>(entity));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null) return NotFound();
        if (!CanEdit(entity)) return Forbid();

        await _repo.DeleteAsync(entity);
        await _repo.SaveChangesAsync();
        return NoContent();
    }

    private int? CurrentCoachIdOrNull()
    {
        return _currentUser.IsCoach ? _currentUser.CoachId : null;
    }

    private bool CanView(Exercise exercise)
    {
        if (_currentUser.IsAdmin) return true;
        if (_currentUser.IsCoach)
        {
            return exercise.IsGlobal || (_currentUser.CoachId.HasValue && exercise.CoachId == _currentUser.CoachId.Value);
        }

        return exercise.IsGlobal;
    }

    private bool CanEdit(Exercise exercise)
    {
        if (_currentUser.IsAdmin) return true;

        return _currentUser.IsCoach
            && _currentUser.CoachId.HasValue
            && !exercise.IsGlobal
            && exercise.CoachId == _currentUser.CoachId.Value;
    }

    private async Task<ActionResult?> ApplyCreateOwnershipAsync(Exercise entity, ExerciseCreateDto input)
    {
        if (_currentUser.IsCoach)
        {
            if (!_currentUser.CoachId.HasValue) return Forbid();

            entity.CoachId = _currentUser.CoachId.Value;
            entity.IsGlobal = false;
            return null;
        }

        if (!_currentUser.IsAdmin) return Forbid();

        return await ApplyAdminOwnershipAsync(entity, input.CoachId, input.IsGlobal);
    }

    private async Task<ActionResult?> ApplyUpdateOwnershipAsync(Exercise entity, ExerciseUpdateDto input)
    {
        if (_currentUser.IsCoach)
        {
            if (!_currentUser.CoachId.HasValue) return Forbid();

            entity.CoachId = _currentUser.CoachId.Value;
            entity.IsGlobal = false;
            return null;
        }

        if (!_currentUser.IsAdmin) return Forbid();

        return await ApplyAdminOwnershipAsync(entity, input.CoachId, input.IsGlobal);
    }

    private async Task<ActionResult?> ApplyAdminOwnershipAsync(Exercise entity, int? coachId, bool isGlobal)
    {
        entity.IsGlobal = isGlobal;
        entity.CoachId = isGlobal ? null : coachId;

        if (entity.CoachId.HasValue && await _coaches.GetByIdAsync(entity.CoachId.Value) is null)
        {
            return BadRequest("CoachId inválido.");
        }

        return null;
    }
}
