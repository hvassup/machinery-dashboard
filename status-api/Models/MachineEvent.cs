namespace status_api.Models;

public class MachineEvent
{
    public int Id { get; set; }
    public required string MachineId { get; set; }
    public string? OrderId { get; set; }
    public required string EventType { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
