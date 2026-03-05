using Contracts.Events;
using MassTransit;
using status_api.Data;
using status_api.Models;
using status_api.Services;

namespace status_api.Consumers;

public class WarmUpProgressEventConsumer(AppDbContext db, PushNotificationService push)
    : IConsumer<WarmUpProgressEvent>
{
    public async Task Consume(ConsumeContext<WarmUpProgressEvent> context)
    {
        var evt = context.Message;
        var machine = await db.Machines.FindAsync(evt.MachineId);
        if (machine == null) return;

        var newStatus = evt.Percentage >= 100 ? "Ready" : "WarmingUp";
        var statusChanged = machine.Status != newStatus;

        machine.WarmUpPercentage = evt.Percentage;
        machine.LastSeen = evt.Timestamp;

        if (statusChanged)
        {
            machine.Status = newStatus;
            db.MachineEvents.Add(new MachineEvent
            {
                MachineId = evt.MachineId,
                EventType = newStatus,
                Timestamp = evt.Timestamp
            });
            await push.SendMachineStatusAsync(evt.MachineId, newStatus);
        }

        await db.SaveChangesAsync();
    }
}
