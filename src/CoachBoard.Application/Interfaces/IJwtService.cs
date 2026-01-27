namespace CoachBoard.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(int userId, string email, string role, int? coachId = null, int? tenantId = null);
}
