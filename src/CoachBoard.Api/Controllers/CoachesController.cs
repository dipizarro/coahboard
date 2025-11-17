using AutoMapper;
using CoachBoard.Application.DTOs;
using CoachBoard.Application.Interfaces;
using CoachBoard.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoachBoard.API.Controllers;

[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Tags("Coaches")]
[Route("api/[controller]")]
public class CoachesController : ControllerBase
{
    private readonly ICoachRepository _repo;
    private readonly IMapper _mapper;

    public CoachesController(ICoachRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<CoachReadDto>>> GetAll()
    {
        var coaches = await _repo.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<CoachReadDto>>(coaches));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<CoachReadDto>> GetById(int id)
    {
        var coach = await _repo.GetByIdAsync(id);
        if (coach is null) return NotFound();

        return Ok(_mapper.Map<CoachReadDto>(coach));
    }

    [HttpPost]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<ActionResult<CoachReadDto>> Create([FromBody] CoachCreateDto input)
    {
        // ModelState ya validado por FluentValidation
        var entity = _mapper.Map<Coach>(input);
        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();

        var read = _mapper.Map<CoachReadDto>(entity);
        return CreatedAtAction(nameof(GetById), new { id = read.Id }, read);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<ActionResult<CoachReadDto>> Update(int id, [FromBody] CoachUpdateDto input)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null) return NotFound();

        _mapper.Map(input, entity);
        await _repo.UpdateAsync(entity);
        await _repo.SaveChangesAsync();

        return Ok(_mapper.Map<CoachReadDto>(entity));
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
