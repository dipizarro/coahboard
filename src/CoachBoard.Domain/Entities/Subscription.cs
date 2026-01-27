using CoachBoard.Domain.Enums;

namespace CoachBoard.Domain.Entities;

public class Subscription : ITenantEntity
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    public string Provider { get; set; } = null!;
    public string ProviderSubscriptionId { get; set; } = null!;
    public SubscriptionStatus Status { get; set; }
    public DateTime? CurrentPeriodEnd { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsActive() => Status == SubscriptionStatus.Active || Status == SubscriptionStatus.Trialing;
}
