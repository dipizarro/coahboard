namespace CoachBoard.Domain.Entities;

public class ClientProgressPhoto
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;
    public int? ClientProgressRecordId { get; set; }
    public ClientProgressRecord? ClientProgressRecord { get; set; }
    public string PhotoUrl { get; set; } = null!;
    public string PhotoType { get; set; } = "Progress";
    public DateTime TakenAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
