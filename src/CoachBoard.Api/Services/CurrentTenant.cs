using System.Security.Claims;
using CoachBoard.Application.Interfaces;

namespace CoachBoard.Api.Services;

public class CurrentTenant : ICurrentTenant
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentTenant(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? TenantId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst("tid") 
                     ?? user?.FindFirst("tenantId") 
                     ?? user?.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid");
            
            if (claim is null) return null;
            
            return int.TryParse(claim.Value, out var id) ? id : null;
        }
    }
}
