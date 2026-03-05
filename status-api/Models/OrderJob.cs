namespace status_api.Models;

public class OrderJob
{
    public required string Id { get; set; }
    public required string MachineId { get; set; }
    public required string ProductId { get; set; }
    public int Quantity { get; set; }
    public string Status { get; set; } = "Scheduled";
    public DateTime ScheduledAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
}
