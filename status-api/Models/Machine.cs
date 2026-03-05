namespace status_api.Models;

public class Machine
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string Status { get; set; } = "Idle";
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    public string? CurrentOrderId { get; set; }
}
