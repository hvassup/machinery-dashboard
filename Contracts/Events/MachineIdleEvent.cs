namespace Contracts.Events;

public class MachineIdleEvent
{
    public required string MachineId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
