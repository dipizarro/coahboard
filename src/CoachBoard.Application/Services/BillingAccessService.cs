using System;
using System.Linq;
using System.Threading.Tasks;
using CoachBoard.Application.Interfaces;
using CoachBoard.Domain.Entities;
using CoachBoard.Domain.Enums;

namespace CoachBoard.Application.Services;

public class BillingAccessService : IBillingAccessService
{
    private readonly IRepository<Subscription> _subscriptionRepository;
    private readonly ICurrentTenant _currentTenant;

    public BillingAccessService(IRepository<Subscription> subscriptionRepository, ICurrentTenant currentTenant)
    {
        _subscriptionRepository = subscriptionRepository;
        _currentTenant = currentTenant;
    }

    public async Task<bool> CanAccessProAsync()
    {
        // If no tenant context, deny (or handle as system admin if needed, but safe default is deny)
        if (!_currentTenant.TenantId.HasValue)
        {
            return false;
        }

        // Get all subscriptions for the current tenant
        // Repository filters by TenantId automatically via ICurrentTenant
        var subscriptions = await _subscriptionRepository.GetAllAsync();
        
        // Find the "best" subscription
        // Priority: Active/Trialing > PastDue/Pending
        var activeSub = subscriptions.FirstOrDefault(s => s.IsActive());

        if (activeSub != null)
        {
            return true;
        }

        // Check for grace period
        // Allow PastDue or Pending IF CurrentPeriodEnd is in the future
        var graceSub = subscriptions.FirstOrDefault(s => 
            (s.Status == SubscriptionStatus.PastDue || s.Status == SubscriptionStatus.Pending) &&
            s.CurrentPeriodEnd.HasValue &&
            s.CurrentPeriodEnd.Value > DateTime.UtcNow);

        return graceSub != null;
    }
}
