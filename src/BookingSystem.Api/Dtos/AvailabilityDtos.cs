using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Api.Dtos;

public class CreateSlotRequest
{
    [Required]
    public int ProviderId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }
}
