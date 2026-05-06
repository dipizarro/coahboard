namespace CoachBoard.Domain.Entities;

public class ClientProgressRecord
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;
    public DateTime RecordedAt { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? BodyFatPercentage { get; set; }
    public decimal? ChestCm { get; set; }
    public decimal? WaistCm { get; set; }
    public decimal? HipCm { get; set; }
    public decimal? LeftArmCm { get; set; }
    public decimal? RightArmCm { get; set; }
    public decimal? LeftThighCm { get; set; }
    public decimal? RightThighCm { get; set; }
    public int? RestingHeartRate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
