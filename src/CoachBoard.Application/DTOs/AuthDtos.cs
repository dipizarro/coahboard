namespace CoachBoard.Application.DTOs;

public record RegisterRequest(string Email, string Password, string Role, string? Name, string? Specialty);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, string Email, string Role, int? CoachId);
