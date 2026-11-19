using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Api.Models;

public class Provider
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Specialty { get; set; }

    // Navigation
    public List<AvailabilitySlot> AvailabilitySlots { get; set; } = new();
    public List<Booking> Bookings { get; set; } = new();
}
