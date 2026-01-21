namespace CoachBoard.Domain.Entities;

public class Session
{
    public int Id { get; set; }

    public int CoachId { get; set; }
    public Coach Coach { get; set; } = null!;

    public int? ClientId { get; set; }
    public Client? Client { get; set; }

    public int? RoutineId { get; set; }
    public Routine? Routine { get; set; }

    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }

    // Planned, Done, Canceled, Missed
    public string Status { get; set; } = "Planned";

    // Training, PersonalBlock, Other
    public string Type { get; set; } = "Training";

    public string? Location { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
}
