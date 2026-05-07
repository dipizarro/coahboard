namespace CoachBoard.Application.DTOs;

public record ClientProgressPhotoCreateDto(
    int? ClientProgressRecordId,
    string PhotoType,
    DateTime TakenAt,
    string? Notes = null);

public record ClientProgressPhotoReadDto(
    int Id,
    int ClientId,
    int? ClientProgressRecordId,
    string PhotoUrl,
    string PhotoType,
    DateTime TakenAt,
    string? Notes,
    DateTime CreatedAt);
