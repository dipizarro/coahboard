using CoachBoard.Domain.Enums;

namespace CoachBoard.Domain.Entities;

public class Tenant
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public SubscriptionPlan Plan { get; set; } = SubscriptionPlan.Free;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
