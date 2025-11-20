namespace CoachBoard.Application.DTOs;

public record SessionCreateDto(
    int? CoachId,        // Para Admin; para Coach se ignora y se usa el token
    int? ClientId,
    int? RoutineId,
    DateTime StartAt,
    DateTime EndAt,
    string Type,
    string? Location,
    string? Notes
);

public record SessionUpdateDto(
    int? ClientId,
    int? RoutineId,
    DateTime StartAt,
    DateTime EndAt,
    string Status,
    string Type,
    string? Location,
    string? Notes
);

public record SessionStatusUpdateDto(string Status);

public class SessionReadDto
{
    public int Id { get; set; }
    public int CoachId { get; set; }
    public int? ClientId { get; set; }
    public string? ClientName { get; set; }
    public int? RoutineId { get; set; }
    public string? RoutineTitle { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string Status { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string? Location { get; set; }
    public string? Notes { get; set; }
};
