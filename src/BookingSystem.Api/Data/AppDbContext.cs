using BookingSystem.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BookingSystem.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<AvailabilitySlot> AvailabilitySlots => Set<AvailabilitySlot>();
    public DbSet<Booking> Bookings => Set<Booking>();

    // SQL Server returns DateTime values with no timezone info. Marking them as UTC means the
    // API serialises them with a trailing 'Z', so browsers convert to the user's local time correctly.
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // A given availability slot should only ever be linked to one booking.
        // Cancelled bookings are excluded (Status 1 = Cancelled) so a freed slot can be rebooked.
        modelBuilder.Entity<Booking>()
            .HasIndex(b => b.AvailabilitySlotId)
            .IsUnique()
            .HasFilter("[Status] <> 1");

        // SQL Server rejects multiple cascade paths: deleting a Provider already cascades
        // to its AvailabilitySlots, which cascade to Bookings. So Booking's direct links to
        // Provider, Service and Customer must NOT cascade. Restrict also means you can't
        // accidentally wipe booking history by deleting a customer or service.
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Provider)
            .WithMany(p => p.Bookings)
            .HasForeignKey(b => b.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Service)
            .WithMany(s => s.Bookings)
            .HasForeignKey(b => b.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Customer)
            .WithMany(c => c.Bookings)
            .HasForeignKey(b => b.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed a little reference data so the API returns something useful on first run.
        modelBuilder.Entity<Provider>().HasData(
            new Provider { Id = 1, Name = "Alex Morgan", Specialty = "Hair Stylist" },
            new Provider { Id = 2, Name = "Sam Rivera", Specialty = "Massage Therapist" }
        );

        modelBuilder.Entity<Service>().HasData(
            new Service { Id = 1, Name = "Haircut", DurationMinutes = 30, Price = 35.00m },
            new Service { Id = 2, Name = "Relaxation Massage", DurationMinutes = 60, Price = 70.00m }
        );
    }
}

public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }
}
