using System;
using System.Threading.Tasks;
using CoachBoard.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace CoachBoard.Api.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireProAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var billingService = context.HttpContext.RequestServices.GetRequiredService<IBillingAccessService>();
        
        var canAccess = await billingService.CanAccessProAsync();
        
        if (!canAccess)
        {
            // Return 403 Forbidden with a clear detail
            context.Result = new ObjectResult(new ProblemDetails
            {
                Status = 403,
                Title = "Pro Plan Required",
                Detail = "This feature requires an active Pro subscription or a valid grace period."
            })
            {
                StatusCode = 403
            };
        }
    }
}
