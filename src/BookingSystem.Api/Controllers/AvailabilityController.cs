using BookingSystem.Api.Data;
using BookingSystem.Api.Dtos;
using BookingSystem.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AvailabilityController : ControllerBase
{
    private readonly AppDbContext _db;

    public AvailabilityController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/availability?providerId=1
    // Returns open (unbooked, future) slots, optionally filtered by provider.
    [HttpGet]
    public async Task<IActionResult> GetOpenSlots([FromQuery] int? providerId)
    {
        var query = _db.AvailabilitySlots
            .Where(s => !s.IsBooked && s.StartTime > DateTime.UtcNow);

        if (providerId is not null)
        {
            query = query.Where(s => s.ProviderId == providerId);
        }

        var slots = await query
            .OrderBy(s => s.StartTime)
            .Select(s => new
            {
                s.Id,
                s.ProviderId,
                s.StartTime,
                s.EndTime
            })
            .ToListAsync();

        return Ok(slots);
    }
    // POST /api/availability
    // Lets a provider/admin publish a new open slot (requirement FR-4).
    [HttpPost]
    public async Task<IActionResult> CreateSlot(CreateSlotRequest request)
    {
        var start = request.StartTime.ToUniversalTime();
        var end = request.EndTime.ToUniversalTime();

        if (end <= start)
        {
            return BadRequest("End time must be after start time.");
        }

        if (start <= DateTime.UtcNow)
        {
            return BadRequest("Slots must start in the future.");
        }

        if (!await _db.Providers.AnyAsync(p => p.Id == request.ProviderId))
        {
            return NotFound("That provider does not exist.");
        }

        // Reject overlaps with the provider's existing slots.
        var overlaps = await _db.AvailabilitySlots.AnyAsync(s =>
            s.ProviderId == request.ProviderId && s.StartTime < end && s.EndTime > start);

        if (overlaps)
        {
            return Conflict("That time overlaps an existing slot for this provider.");
        }

        var slot = new AvailabilitySlot
        {
            ProviderId = request.ProviderId,
            StartTime = start,
            EndTime = end
        };

        _db.AvailabilitySlots.Add(slot);
        await _db.SaveChangesAsync();

        return Created($"/api/availability/{slot.Id}", new { slot.Id, slot.ProviderId, slot.StartTime, slot.EndTime });
    }
}
