using System.Security.Claims;

namespace CoachBoard.Application.Interfaces;

public interface ICurrentUserService
{
    ClaimsPrincipal? User { get; }

    string? Email { get; }
    string? Role { get; }
    int? CoachId { get; }

    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    bool IsCoach { get; }
}
