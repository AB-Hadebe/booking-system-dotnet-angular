namespace BookingSystem.Api.Models;

public class Booking
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int ProviderId { get; set; }
    public Provider? Provider { get; set; }

    public int ServiceId { get; set; }
    public Service? Service { get; set; }

    public int AvailabilitySlotId { get; set; }
    public AvailabilitySlot? AvailabilitySlot { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
