using Contracts.Events;
using MassTransit;
using status_api.Data;
using status_api.Models;
using status_api.Services;

namespace status_api.Consumers;

public class MachineCoolingDownEventConsumer(AppDbContext db, PushNotificationService push)
    : IConsumer<MachineCoolingDownEvent>
{
    public async Task Consume(ConsumeContext<MachineCoolingDownEvent> context)
    {
        var evt = context.Message;
        var machine = await db.Machines.FindAsync(evt.MachineId);
        if (machine == null) return;

        machine.Status = "CoolingDown";
        machine.LastSeen = evt.Timestamp;
        db.MachineEvents.Add(new MachineEvent
        {
            MachineId = evt.MachineId,
            EventType = "CoolingDown",
            Timestamp = evt.Timestamp
        });

        await db.SaveChangesAsync();
        await push.SendMachineStatusAsync(evt.MachineId, "CoolingDown");
    }
}
