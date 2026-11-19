using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Api.Models;

public class Customer
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Phone { get; set; }

    // Navigation
    public List<Booking> Bookings { get; set; } = new();
}
