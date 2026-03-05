namespace Contracts.Events;

public class WarmUpProgressEvent
{
    public required string MachineId { get; set; }
    public int Percentage { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
