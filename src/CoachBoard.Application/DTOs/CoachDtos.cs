namespace CoachBoard.Application.DTOs;

public record CoachCreateDto(string Name, string Specialty);
public record CoachUpdateDto(string Name, string Specialty);
public record CoachReadDto(int Id, string Name, string Specialty, DateTime CreatedAt);
