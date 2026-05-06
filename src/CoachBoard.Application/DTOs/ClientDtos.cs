namespace CoachBoard.Application.DTOs;

public record ClientCreateDto(
    string FullName,
    string? Email,
    string? Phone,
    int CoachId,
    DateTime? BirthDate = null,
    string? Gender = null,
    decimal? InitialHeightCm = null,
    string? MainGoal = null,
    string? ExperienceLevel = null,
    string? MedicalNotes = null,
    string? InjuryNotes = null,
    string? GeneralNotes = null,
    DateTime? StartDate = null,
    bool IsActive = true);

public record ClientUpdateDto(
    string FullName,
    string? Email,
    string? Phone,
    DateTime? BirthDate = null,
    string? Gender = null,
    decimal? InitialHeightCm = null,
    string? MainGoal = null,
    string? ExperienceLevel = null,
    string? MedicalNotes = null,
    string? InjuryNotes = null,
    string? GeneralNotes = null,
    DateTime? StartDate = null,
    bool IsActive = true);

public record ClientReadDto(
    int Id,
    string FullName,
    string? Email,
    string? Phone,
    int CoachId,
    DateTime CreatedAt,
    DateTime? BirthDate = null,
    string? Gender = null,
    decimal? InitialHeightCm = null,
    string? MainGoal = null,
    string? ExperienceLevel = null,
    string? MedicalNotes = null,
    string? InjuryNotes = null,
    string? GeneralNotes = null,
    DateTime? StartDate = null,
    bool IsActive = true);

public record PagedResult<T>(IEnumerable<T> Items, int Total, int Page, int PageSize);
