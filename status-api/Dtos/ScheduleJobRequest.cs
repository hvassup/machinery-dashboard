namespace status_api.Dtos;

public class ScheduleJobRequest
{
    public required string MachineId { get; set; }
    public required string ProductId { get; set; }
    public int Quantity { get; set; }
}