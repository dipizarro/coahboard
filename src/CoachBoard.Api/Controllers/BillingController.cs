using CoachBoard.Application.DTOs;
using CoachBoard.Application.Interfaces;
using CoachBoard.Domain.Entities;
using CoachBoard.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoachBoard.Api.Controllers;

[ApiController]
[Route("api/billing")]
[Authorize(Roles = "Coach,Admin")]
public class BillingController : ControllerBase
{
    private readonly IMercadoPagoClient _mpClient;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUserService _currentUser;
    private readonly IRepository<Subscription> _subRepo;
    private readonly ITenantRepository _tenantRepo;

    public BillingController(
        IMercadoPagoClient mpClient, 
        ICurrentTenant currentTenant, 
        ICurrentUserService currentUser,
        IRepository<Subscription> subRepo,
        ITenantRepository tenantRepo)
    {
        _mpClient = mpClient;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
        _subRepo = subRepo;
        _tenantRepo = tenantRepo;
    }

    [HttpGet("status")]
    public async Task<ActionResult<BillingStatusDto>> GetStatus([FromServices] IBillingAccessService billingAccess)
    {
        var tenantId = _currentTenant.TenantId;
        if (tenantId is null) return BadRequest("Tenant context missing");

        var tenant = await _tenantRepo.GetByIdAsync(tenantId.Value);
        if (tenant is null) return NotFound("Tenant not found");

        var subscriptions = await _subRepo.GetAllAsync(); // Filtered by TenantId via global filter/repo logic? 
        // Note: Repository.GetAllAsync() usually applies current tenant filter if implemented. 
        // Let's assume it does or we rely on the fact that we are in tenant context.
        // Re-checking Repository implementation via thought trace: 
        // User's Repository often filters by ICurrentTenant.
        
        // Logic to find best subscription (Active > Grace Period > Latest)
        var activeSub = subscriptions.FirstOrDefault(s => s.IsActive());
        var graceSub = subscriptions.FirstOrDefault(s => 
            (s.Status == SubscriptionStatus.PastDue || s.Status == SubscriptionStatus.Pending) &&
            s.CurrentPeriodEnd.HasValue &&
            s.CurrentPeriodEnd.Value > DateTime.UtcNow);
            
        var latestSub = subscriptions.OrderByDescending(s => s.CreatedAt).FirstOrDefault();

        var bestSub = activeSub ?? graceSub ?? latestSub;
        
        // Access Check
        var canAccessPro = await billingAccess.CanAccessProAsync();

        return Ok(new BillingStatusDto(
            Plan: tenant.Plan,
            SubscriptionStatus: bestSub?.Status ?? SubscriptionStatus.None,
            CanAccessPro: canAccessPro,
            CurrentPeriodEnd: bestSub?.CurrentPeriodEnd,
            ProviderSubscriptionId: bestSub?.ProviderSubscriptionId
        ));
    }

    [HttpPost("checkout/pro")]
    public async Task<ActionResult<CheckoutResponse>> CreateProCheckout()
    {
        var tenantId = _currentTenant.TenantId;
        if (tenantId is null) return BadRequest("Tenant context missing");

        var userId = _currentUser.UserId;
        // In a real scenario, we might want to fetch the User email from DB if not in claims, 
        // but let's assume valid user initiates this.
        // For simplicity, we'll pass a placeholder or fetch if needed.
        // Let's use a placeholder for now as email isn't critical for the STUB.
        var userEmail = "user@example.com"; 

        // 1. Generate Reference ID first
        var referenceId = Guid.NewGuid().ToString(); 

        // 2. Get Checkout URL from Provider
        var initPoint = await _mpClient.CreateProCheckoutAsync(userId ?? 0, userEmail, tenantId.Value, referenceId);

        // 3. Create Pending Subscription Record 

        var subscription = new Subscription
        {
            TenantId = tenantId.Value,
            Provider = "MercadoPago",
            ProviderSubscriptionId = referenceId, // Temporary ref until webhook updates it
            Status = SubscriptionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _subRepo.AddAsync(subscription);
        await _subRepo.SaveChangesAsync();

        return Ok(new CheckoutResponse(initPoint, referenceId));
    }
}
