namespace BookingSystem.Api.Models;

public class AvailabilitySlot
{
    public int Id { get; set; }

    public int ProviderId { get; set; }
    public Provider? Provider { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public bool IsBooked { get; set; } = false;

    // A slot is free if it isn't already marked booked and hasn't already passed.
    public bool IsAvailable() => !IsBooked && StartTime > DateTime.UtcNow;
}
