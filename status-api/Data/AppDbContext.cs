using Microsoft.EntityFrameworkCore;
using status_api.Models;

namespace status_api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Machine> Machines { get; set; }
    public DbSet<OrderJob> OrderJobs { get; set; }
    public DbSet<MachineEvent> MachineEvents { get; set; }
    public DbSet<DeviceSubscription> DeviceSubscriptions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Machine>().HasKey(m => m.Id);
        modelBuilder.Entity<OrderJob>().HasKey(o => o.Id);
        modelBuilder.Entity<MachineEvent>().HasKey(e => e.Id);
        modelBuilder.Entity<MachineEvent>().Property(e => e.Id).ValueGeneratedOnAdd();
    }
}
