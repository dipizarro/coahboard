using AutoMapper;
using CoachBoard.Application.DTOs;
using CoachBoard.Application.Interfaces;
using CoachBoard.Application.Models;
using CoachBoard.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CoachBoard.Api.Controllers;

[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Tags("ExerciseMedia")]
[Route("api/exercises/{exerciseId:int}/media")]
[Authorize(Roles = "Coach,Admin")]
public class ExerciseMediaController : ControllerBase
{
    private readonly IExerciseRepository _exercises;
    private readonly IExerciseMediaRepository _media;
    private readonly IFileStorageService _storage;
    private readonly ICurrentUserService _currentUser;
    private readonly FileStorageOptions _storageOptions;
    private readonly IMapper _mapper;

    public ExerciseMediaController(
        IExerciseRepository exercises,
        IExerciseMediaRepository media,
        IFileStorageService storage,
        ICurrentUserService currentUser,
        IOptions<FileStorageOptions> storageOptions,
        IMapper mapper)
    {
        _exercises = exercises;
        _media = media;
        _storage = storage;
        _currentUser = currentUser;
        _storageOptions = storageOptions.Value;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExerciseMediaReadDto>>> GetByExercise(int exerciseId)
    {
        var exercise = await _exercises.GetByIdAsync(exerciseId);
        if (exercise is null) return NotFound();
        if (!CanView(exercise)) return NotFound();

        var media = await _media.GetByExerciseAsync(exerciseId);
        return Ok(_mapper.Map<IEnumerable<ExerciseMediaReadDto>>(media));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ExerciseMediaReadDto>> Create(
        int exerciseId,
        [FromForm] ExerciseMediaUploadRequest input,
        CancellationToken cancellationToken)
    {
        var exercise = await _exercises.GetByIdAsync(exerciseId);
        if (exercise is null) return NotFound();
        if (!CanEdit(exercise)) return Forbid();

        var fileValidationError = ValidateFile(input.File);
        if (fileValidationError is not null) return BadRequest(fileValidationError);

        if (!string.IsNullOrWhiteSpace(input.Title) && input.Title.Length > 150)
            return BadRequest("Title no puede superar 150 caracteres.");

        if (!string.IsNullOrWhiteSpace(input.Description) && input.Description.Length > 500)
            return BadRequest("Description no puede superar 500 caracteres.");

        var url = await _storage.SaveAsync(
            input.File!.OpenReadStream(),
            input.File.FileName,
            input.File.ContentType,
            cancellationToken);

        var createDto = new ExerciseMediaCreateDto(
            "Image",
            url,
            string.IsNullOrWhiteSpace(input.Title) ? null : input.Title.Trim(),
            string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim());

        var entity = _mapper.Map<ExerciseMedia>(createDto);
        entity.ExerciseId = exerciseId;

        await _media.AddAsync(entity);
        await _media.SaveChangesAsync();

        return Created($"/api/exercises/{exerciseId}/media/{entity.Id}", _mapper.Map<ExerciseMediaReadDto>(entity));
    }

    [HttpDelete("{mediaId:int}")]
    public async Task<IActionResult> Delete(int exerciseId, int mediaId, CancellationToken cancellationToken)
    {
        var exercise = await _exercises.GetByIdAsync(exerciseId);
        if (exercise is null) return NotFound();
        if (!CanEdit(exercise)) return Forbid();

        var entity = await _media.GetByExerciseAndIdAsync(exerciseId, mediaId);
        if (entity is null) return NotFound();

        await _media.DeleteAsync(entity);
        await _media.SaveChangesAsync();
        await _storage.DeleteAsync(entity.Url, cancellationToken);

        return NoContent();
    }

    private string? ValidateFile(IFormFile? file)
    {
        if (file is null || file.Length == 0)
            return "El archivo es requerido.";

        if (_storageOptions.MaxFileSizeBytes > 0 && file.Length > _storageOptions.MaxFileSizeBytes)
            return $"El archivo supera el tamaño máximo permitido de {_storageOptions.MaxFileSizeBytes} bytes.";

        var extension = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
        var allowed = _storageOptions.AllowedExtensions
            .Select(x => x.Trim().TrimStart('.').ToLowerInvariant())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet();

        if (string.IsNullOrWhiteSpace(extension) || !allowed.Contains(extension))
            return $"Extensión no permitida. Usa: {string.Join(", ", allowed)}.";

        return null;
    }

    private bool CanView(Exercise exercise)
    {
        if (_currentUser.IsAdmin) return true;
        if (_currentUser.IsCoach)
        {
            return exercise.IsGlobal || (_currentUser.CoachId.HasValue && exercise.CoachId == _currentUser.CoachId.Value);
        }

        return false;
    }

    private bool CanEdit(Exercise exercise)
    {
        if (_currentUser.IsAdmin) return true;

        return _currentUser.IsCoach
            && _currentUser.CoachId.HasValue
            && !exercise.IsGlobal
            && exercise.CoachId == _currentUser.CoachId.Value;
    }
}

public class ExerciseMediaUploadRequest
{
    public IFormFile? File { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
}
