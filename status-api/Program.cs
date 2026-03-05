using MassTransit;
using Microsoft.EntityFrameworkCore;
using status_api.Consumers;
using status_api.Data;
using status_api.Services;

var builder = WebApplication.CreateBuilder(args);

var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
var rabbitUser = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest";
var rabbitPass = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "guest";

var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
    ?? builder.Configuration.GetConnectionString("Default")
    ?? "Host=localhost;Database=machinery;Username=machinery;Password=machinery";

builder.Services.AddScoped<PushNotificationService>();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(connectionString));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<WarmUpProgressEventConsumer>();
    x.AddConsumer<CoolDownProgressEventConsumer>();
    x.AddConsumer<BeginningOrderEventConsumer>();
    x.AddConsumer<FinishedOrderEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(rabbitUser);
            h.Password(rabbitPass);
        });

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    var machineIds = builder.Configuration.GetSection("Machines").Get<string[]>()
        ?? ["machine-01", "machine-02", "machine-03"];

    foreach (var id in machineIds)
    {
        if (!db.Machines.Any(m => m.Id == id))
        {
            db.Machines.Add(new status_api.Models.Machine
            {
                Id = id,
                Name = id.Replace("-", " ").ToUpperInvariant(),
                Status = "Idle",
                LastSeen = DateTime.UtcNow
            });
        }
    }
    db.SaveChanges();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
