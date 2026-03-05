namespace Contracts.Events;

public abstract class MachineEventBase
{
    public required string MachineId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
