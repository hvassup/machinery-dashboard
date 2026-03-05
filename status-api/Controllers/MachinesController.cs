using Contracts.Commands;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using status_api.Data;
using status_api.Dtos;
using status_api.Models;

namespace status_api.Controllers;

[ApiController]
public class MachinesController(AppDbContext db, ISendEndpointProvider sendEndpointProvider) : ControllerBase
{
    [HttpGet("machines")]
    public async Task<IActionResult> GetMachines()
    {
        var machines = await db.Machines.OrderBy(m => m.Name).ToListAsync();
        var result = machines.Select(m => new
        {
            machineId = m.Id,
            name = m.Name,
            status = m.Status,
            lastSeen = m.LastSeen,
            currentOrderId = m.CurrentOrderId
        });
        return Ok(result);
    }

    [HttpGet("machine/{id}")]
    public async Task<IActionResult> GetMachine(string id)
    {
        var machine = await db.Machines.FindAsync(id);
        if (machine == null) return NotFound();

        var history = await db.MachineEvents
            .Where(e => e.MachineId == id)
            .OrderByDescending(e => e.Timestamp)
            .Take(50)
            .Select(e => new { eventType = e.EventType, orderId = e.OrderId, timestamp = e.Timestamp })
            .ToListAsync();

        var scheduledOrders = await db.OrderJobs
            .Where(o => o.MachineId == id && o.Status == "Scheduled")
            .OrderBy(o => o.ScheduledAt)
            .Select(o => new { orderId = o.Id, productId = o.ProductId, quantity = o.Quantity, scheduledAt = o.ScheduledAt })
            .ToListAsync();

        var historicalOrders = await db.OrderJobs
            .Where(o => o.MachineId == id && o.Status == "Finished")
            .OrderByDescending(o => o.FinishedAt)
            .Take(20)
            .Select(o => new { orderId = o.Id, productId = o.ProductId, quantity = o.Quantity, startedAt = o.StartedAt, finishedAt = o.FinishedAt })
            .ToListAsync();

        return Ok(new
        {
            machineId = machine.Id,
            name = machine.Name,
            status = machine.Status,
            lastSeen = machine.LastSeen,
            currentOrderId = machine.CurrentOrderId,
            scheduledOrders,
            historicalOrders,
            history
        });
    }

    [HttpPost("schedulejob")]
    public async Task<IActionResult> ScheduleJob([FromBody] ScheduleJobRequest request)
    {
        var machine = await db.Machines.FindAsync(request.MachineId);
        if (machine == null) return NotFound($"Machine '{request.MachineId}' not found.");

        var orderId = Guid.NewGuid().ToString();
        db.OrderJobs.Add(new OrderJob
        {
            Id = orderId,
            MachineId = request.MachineId,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            Status = "Scheduled",
            ScheduledAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var endpoint = await sendEndpointProvider.GetSendEndpoint(
            new Uri($"queue:equipment-{request.MachineId}"));

        await endpoint.Send(new BeginProcessOrderCommand
        {
            MachineId = request.MachineId,
            OrderId = orderId,
            ProductId = request.ProductId,
            Quantity = request.Quantity
        });

        return Accepted(new { orderId });
    }
}