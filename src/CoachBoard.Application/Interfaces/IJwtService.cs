namespace CoachBoard.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(string email, string role, int? coachId = null);
}
