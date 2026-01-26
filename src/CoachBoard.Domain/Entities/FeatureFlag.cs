namespace CoachBoard.Domain.Entities;

public class FeatureFlag : ITenantEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsEnabled { get; set; }
    
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
}
