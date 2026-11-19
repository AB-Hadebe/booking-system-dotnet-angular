using Microsoft.EntityFrameworkCore;
using BookingSystem.Api.Models;

namespace BookingSystem.Api.Data;

public static class DbSeeder
{
    // Development helper: makes sure every provider has hourly slots for the next 7 days
    // (Mon-Fri). Safe to run on every startup - it only adds slots that are missing.
    // Times are stored in UTC: 07:00-13:00 UTC starts = 09:00-15:00 in South Africa (UTC+2).
    public static async Task SeedAvailabilityAsync(AppDbContext db)
    {
        var providerIds = await db.Providers.Select(p => p.Id).ToListAsync();
        var today = DateTime.UtcNow.Date;

        var existing = await db.AvailabilitySlots
            .Where(s => s.StartTime >= today)
            .Select(s => new { s.ProviderId, s.StartTime })
            .ToListAsync();
        var existingKeys = existing.Select(e => (e.ProviderId, e.StartTime)).ToHashSet();

        foreach (var providerId in providerIds)
        {
            for (var dayOffset = 1; dayOffset <= 7; dayOffset++)
            {
                var day = today.AddDays(dayOffset);
                if (day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                {
                    continue;
                }

                for (var hour = 7; hour <= 13; hour++)
                {
                    var start = DateTime.SpecifyKind(day.AddHours(hour), DateTimeKind.Utc);
                    if (existingKeys.Contains((providerId, start)))
                    {
                        continue;
                    }

                    db.AvailabilitySlots.Add(new AvailabilitySlot
                    {
                        ProviderId = providerId,
                        StartTime = start,
                        EndTime = start.AddHours(1)
                    });
                }
            }
        }

        await db.SaveChangesAsync();
    }
}
