namespace Contracts.Events;

public class MachineWarmingUpEvent
{
    public required string MachineId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
