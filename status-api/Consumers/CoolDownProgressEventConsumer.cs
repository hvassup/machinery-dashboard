using Contracts.Events;
using MassTransit;
using status_api.Data;
using status_api.Models;
using status_api.Services;

namespace status_api.Consumers;

public class CoolDownProgressEventConsumer(AppDbContext db, PushNotificationService push)
    : IConsumer<CoolDownProgressEvent>
{
    public async Task Consume(ConsumeContext<CoolDownProgressEvent> context)
    {
        var evt = context.Message;
        var machine = await db.Machines.FindAsync(evt.MachineId);
        if (machine == null) return;

        var newStatus = evt.Percentage <= 0 ? "Idle" : "CoolingDown";
        var statusChanged = machine.Status != newStatus;

        machine.WarmUpPercentage = evt.Percentage;
        machine.LastSeen = evt.Timestamp;

        if (statusChanged)
        {
            machine.Status = newStatus;
            if (newStatus == "Idle") machine.CurrentOrderId = null;
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
