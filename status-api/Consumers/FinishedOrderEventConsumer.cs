using Contracts.Events;
using MassTransit;
using status_api.Data;
using status_api.Models;
using status_api.Services;

namespace status_api.Consumers;

public class FinishedOrderEventConsumer(AppDbContext db, PushNotificationService push, ILogger<FinishedOrderEventConsumer> logger)
    : IConsumer<FinishedOrderEvent>
{
    public async Task Consume(ConsumeContext<FinishedOrderEvent> context)
    {
        var evt = context.Message;
        var machine = await db.Machines.FindAsync(evt.MachineId);
        if (machine != null)
        {
            machine.Status = "Ready";
            machine.LastSeen = evt.Timestamp;
            machine.CurrentOrderId = null;
        }

        var job = await db.OrderJobs.FindAsync(evt.OrderId);
        if (job != null)
        {
            job.Status = "Finished";
            job.FinishedAt = evt.Timestamp;
        }

        db.MachineEvents.Add(new MachineEvent
        {
            MachineId = evt.MachineId,
            OrderId = evt.OrderId,
            EventType = "FinishedOrder",
            Timestamp = evt.Timestamp
        });

        await db.SaveChangesAsync();
        await push.SendMachineStatusAsync(evt.MachineId, "Ready");
        logger.LogInformation("Machine {MachineId} finished order {OrderId}", evt.MachineId, evt.OrderId);
    }
}
