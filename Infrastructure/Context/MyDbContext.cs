using Microsoft.EntityFrameworkCore;
using MinimalApi.Domain.Entities;

namespace MinimalApi.Infrastructure.Context;

public class MyDbContext : DbContext
{
    private readonly IConfiguration _configurationAppSettings;
    public MyDbContext(IConfiguration configurationAppSettings)
    {
        _configurationAppSettings = configurationAppSettings;
    }

    public DbSet<Administrator> Administrators { get; set; } = default!;
    public DbSet<Vehicle> Vehicles { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Administrator>().HasData(
            new Administrator {
                Id = 1,
                Email = "Administrator@teste.com",
                Password = "123456",
                Profile = "Adm"
             }
        );
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if(!optionsBuilder.IsConfigured)
        {
            var connectionString = _configurationAppSettings.GetConnectionString("DefaultConnection")?.ToString();
            if(!string.IsNullOrEmpty(connectionString))
            {
                optionsBuilder.UseSqlServer(connectionString);
            }
        }
    }
}