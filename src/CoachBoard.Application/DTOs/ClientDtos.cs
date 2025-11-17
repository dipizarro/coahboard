namespace CoachBoard.Application.DTOs;

public record ClientCreateDto(string FullName, string? Email, string? Phone, int CoachId);
public record ClientUpdateDto(string FullName, string? Email, string? Phone);
public record ClientReadDto(int Id, string FullName, string? Email, string? Phone, int CoachId, DateTime CreatedAt);

public record PagedResult<T>(IEnumerable<T> Items, int Total, int Page, int PageSize);
