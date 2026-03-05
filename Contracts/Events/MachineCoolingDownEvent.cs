namespace Contracts.Events;

public class MachineCoolingDownEvent
{
    public required string MachineId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
