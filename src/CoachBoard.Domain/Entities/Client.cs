namespace CoachBoard.Domain.Entities;

public class Client
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int CoachId { get; set; }
    public Coach Coach { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Routine> Routines { get; set; }
}
