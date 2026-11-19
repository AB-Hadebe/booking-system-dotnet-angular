using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Api.Dtos;

public class CreateBookingRequest
{
    [Required]
    public string CustomerName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string CustomerEmail { get; set; } = string.Empty;

    public int ServiceId { get; set; }

    public int AvailabilitySlotId { get; set; }
}

public class BookingResponse
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
}
