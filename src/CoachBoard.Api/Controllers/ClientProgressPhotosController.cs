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
[Tags("ClientProgressPhotos")]
[Route("api/clients/{clientId:int}/photos")]
[Authorize(Roles = "Coach,Admin")]
public class ClientProgressPhotosController : ControllerBase
{
    private readonly IClientRepository _clients;
    private readonly IClientProgressRepository _progress;
    private readonly IClientProgressPhotoRepository _photos;
    private readonly IFileStorageService _storage;
    private readonly ICurrentUserService _currentUser;
    private readonly FileStorageOptions _storageOptions;
    private readonly IMapper _mapper;

    public ClientProgressPhotosController(
        IClientRepository clients,
        IClientProgressRepository progress,
        IClientProgressPhotoRepository photos,
        IFileStorageService storage,
        ICurrentUserService currentUser,
        IOptions<FileStorageOptions> storageOptions,
        IMapper mapper)
    {
        _clients = clients;
        _progress = progress;
        _photos = photos;
        _storage = storage;
        _currentUser = currentUser;
        _storageOptions = storageOptions.Value;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClientProgressPhotoReadDto>>> GetByClient(int clientId)
    {
        var client = await _clients.GetByIdAsync(clientId);
        if (client is null) return NotFound();
        if (!CurrentCoachCanAccessClient(client)) return Forbid();

        var photos = await _photos.GetByClientAsync(clientId);
        return Ok(_mapper.Map<IEnumerable<ClientProgressPhotoReadDto>>(photos));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ClientProgressPhotoReadDto>> Create(
        int clientId,
        [FromForm] ClientProgressPhotoUploadRequest input,
        CancellationToken cancellationToken)
    {
        var client = await _clients.GetByIdAsync(clientId);
        if (client is null) return NotFound();
        if (!CurrentCoachCanAccessClient(client)) return Forbid();

        var fileValidationError = ValidateFile(input.File);
        if (fileValidationError is not null) return BadRequest(fileValidationError);

        if (input.ClientProgressRecordId.HasValue)
        {
            var progressRecord = await _progress.GetByClientAndIdAsync(clientId, input.ClientProgressRecordId.Value);
            if (progressRecord is null) return BadRequest("ClientProgressRecordId inválido para este cliente.");
        }

        if (!string.IsNullOrWhiteSpace(input.PhotoType) && input.PhotoType.Length > 30)
            return BadRequest("PhotoType no puede superar 30 caracteres.");

        if (!string.IsNullOrWhiteSpace(input.Notes) && input.Notes.Length > 500)
            return BadRequest("Notes no puede superar 500 caracteres.");

        var createDto = new ClientProgressPhotoCreateDto(
            input.ClientProgressRecordId,
            string.IsNullOrWhiteSpace(input.PhotoType) ? "Progress" : input.PhotoType.Trim(),
            input.TakenAt ?? DateTime.UtcNow,
            input.Notes);

        var photoUrl = await _storage.SaveAsync(
            input.File!.OpenReadStream(),
            input.File.FileName,
            input.File.ContentType,
            cancellationToken);

        var entity = _mapper.Map<ClientProgressPhoto>(createDto);
        entity.ClientId = clientId;
        entity.PhotoUrl = photoUrl;

        await _photos.AddAsync(entity);
        await _photos.SaveChangesAsync();

        return Created($"/api/clients/{clientId}/photos/{entity.Id}", _mapper.Map<ClientProgressPhotoReadDto>(entity));
    }

    [HttpDelete("{photoId:int}")]
    public async Task<IActionResult> Delete(int clientId, int photoId, CancellationToken cancellationToken)
    {
        var client = await _clients.GetByIdAsync(clientId);
        if (client is null) return NotFound();
        if (!CurrentCoachCanAccessClient(client)) return Forbid();

        var entity = await _photos.GetByClientAndIdAsync(clientId, photoId);
        if (entity is null) return NotFound();

        await _photos.DeleteAsync(entity);
        await _photos.SaveChangesAsync();
        await _storage.DeleteAsync(entity.PhotoUrl, cancellationToken);

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

    private bool CurrentCoachCanAccessClient(Client client)
    {
        if (!_currentUser.IsCoach) return true;

        return _currentUser.CoachId is not null && client.CoachId == _currentUser.CoachId.Value;
    }
}

public class ClientProgressPhotoUploadRequest
{
    public IFormFile? File { get; set; }
    public int? ClientProgressRecordId { get; set; }
    public string? PhotoType { get; set; }
    public DateTime? TakenAt { get; set; }
    public string? Notes { get; set; }
}
