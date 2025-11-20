using System.Security.Claims;
using CoachBoard.Application.Interfaces;

namespace CoachBoard.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public string? Email =>
        User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User?.FindFirst(ClaimTypes.Name)?.Value
        ?? User?.FindFirst("sub")?.Value;

    public string? Role =>
        User?.FindFirst(ClaimTypes.Role)?.Value;

    public int? CoachId
    {
        get
        {
            var claim = User?.FindFirst("coachId");
            if (claim is null) return null;
            return int.TryParse(claim.Value, out var id) ? id : null;
        }
    }

    public bool IsAdmin => string.Equals(Role, "Admin", StringComparison.OrdinalIgnoreCase);
    public bool IsCoach => string.Equals(Role, "Coach", StringComparison.OrdinalIgnoreCase);
}
