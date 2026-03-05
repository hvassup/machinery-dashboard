using System.Threading.Channels;
using Contracts.Commands;
using equipment_service;
using equipment_service.CommandHandlers;
using equipment_service.Options;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);

var equipmentId = Environment.GetEnvironmentVariable("EQUIPMENT_ID")
    ?? builder.Configuration["EquipmentId"]
    ?? "equipment-default";

var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
var rabbitUser = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest";
var rabbitPass = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "guest";

builder.Services.Configure<MachineWorkerOptions>(builder.Configuration);
builder.Services.AddSingleton(Channel.CreateUnbounded<BeginProcessOrderCommand>());
builder.Services.AddHostedService<MachineWorker>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<BeginProcessOrderCommandConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(rabbitUser);
            h.Password(rabbitPass);
        });

        cfg.ReceiveEndpoint($"equipment-{equipmentId}", e =>
        {
            e.ConfigureConsumer<BeginProcessOrderCommandConsumer>(context);
        });
    });
});

var host = builder.Build();
host.Run();
