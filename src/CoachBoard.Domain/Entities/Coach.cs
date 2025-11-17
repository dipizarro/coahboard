namespace CoachBoard.Domain.Entities;

public class Coach
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public User? User { get; set; }
    public string Name { get; set; } = null!;
    public string Specialty { get; set; } = "General";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Client> Clients { get; set; }
}
