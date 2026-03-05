using Contracts.Events;
using MassTransit;
using status_api.Data;
using status_api.Models;
using status_api.Services;

namespace status_api.Consumers;

public class BeginningOrderEventConsumer(AppDbContext db, PushNotificationService push, ILogger<BeginningOrderEventConsumer> logger)
    : IConsumer<BeginningOrderEvent>
{
    public async Task Consume(ConsumeContext<BeginningOrderEvent> context)
    {
        var evt = context.Message;
        var machine = await db.Machines.FindAsync(evt.MachineId);
        if (machine != null)
        {
            machine.Status = "Processing";
            machine.LastSeen = evt.Timestamp;
            machine.CurrentOrderId = evt.OrderId;
        }

        var job = await db.OrderJobs.FindAsync(evt.OrderId);
        if (job != null)
        {
            job.Status = "Processing";
            job.StartedAt = evt.Timestamp;
        }

        db.MachineEvents.Add(new MachineEvent
        {
            MachineId = evt.MachineId,
            OrderId = evt.OrderId,
            EventType = "BeginningOrder",
            Timestamp = evt.Timestamp
        });

        await db.SaveChangesAsync();
        await push.SendMachineStatusAsync(evt.MachineId, "Processing");
        logger.LogInformation("Machine {MachineId} began processing order {OrderId}", evt.MachineId, evt.OrderId);
    }
}
