namespace CoachBoard.Domain.Entities;

public class Client : ITenantEntity
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Gender { get; set; }
    public decimal? InitialHeightCm { get; set; }
    public string? MainGoal { get; set; }
    public string? ExperienceLevel { get; set; }
    public string? MedicalNotes { get; set; }
    public string? InjuryNotes { get; set; }
    public string? GeneralNotes { get; set; }
    public DateTime? StartDate { get; set; }
    public bool IsActive { get; set; } = true;
    public int CoachId { get; set; }
    public Coach Coach { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Routine> Routines { get; set; }
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    public ICollection<ClientProgressRecord> ProgressRecords { get; set; } = new List<ClientProgressRecord>();

    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
}
