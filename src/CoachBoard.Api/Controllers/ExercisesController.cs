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
    private readonly IMapper _mapper;

    public ExercisesController(IExerciseRepository repo, IMapper mapper)
    {
        _repo = repo; _mapper = mapper;
    }

    // GET /api/exercises?q=press&category=fuerza&page=1&pageSize=20
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<ExerciseReadDto>>> Search([FromQuery] string? q, [FromQuery] string? category, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 20 : pageSize;

        var items = await _repo.SearchAsync(q, category, page, pageSize);
        var total = await _repo.CountAsync(q, category);
        var dto = _mapper.Map<IEnumerable<ExerciseReadDto>>(items);

        return Ok(new PagedResult<ExerciseReadDto>(dto, total, page, pageSize));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ExerciseReadDto>> GetById(int id)
    {
        var ex = await _repo.GetByIdAsync(id);
        return ex is null ? NotFound() : Ok(_mapper.Map<ExerciseReadDto>(ex));
    }

    [HttpPost]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<ActionResult<ExerciseReadDto>> Create([FromBody] ExerciseCreateDto input)
    {
        var entity = _mapper.Map<Exercise>(input);
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

        _mapper.Map(input, entity);
        await _repo.UpdateAsync(entity);
        await _repo.SaveChangesAsync();
        return Ok(_mapper.Map<ExerciseReadDto>(entity));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null) return NotFound();

        await _repo.DeleteAsync(entity);
        await _repo.SaveChangesAsync();
        return NoContent();
    }
}
