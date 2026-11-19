using BookingSystem.Api.Data;
using BookingSystem.Api.Dtos;
using BookingSystem.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly AppDbContext _db;

    public BookingsController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/bookings/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookingResponse>> GetById(int id)
    {
        var booking = await _db.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Provider)
            .Include(b => b.Service)
            .Include(b => b.AvailabilitySlot)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(booking));
    }

    // POST /api/bookings
    // Creates (or reuses) a customer by email, then books the requested slot
    // for the requested service - if that slot is still free.
    [HttpPost]
    public async Task<ActionResult<BookingResponse>> Create(CreateBookingRequest request)
    {
        var slot = await _db.AvailabilitySlots
            .Include(s => s.Provider)
            .FirstOrDefaultAsync(s => s.Id == request.AvailabilitySlotId);

        if (slot is null)
        {
            return NotFound("That availability slot does not exist.");
        }

        if (!slot.IsAvailable())
        {
            return Conflict("That slot is no longer available.");
        }

        var service = await _db.Services.FindAsync(request.ServiceId);
        if (service is null)
        {
            return NotFound("That service does not exist.");
        }

        var customer = await _db.Customers
            .FirstOrDefaultAsync(c => c.Email == request.CustomerEmail);

        if (customer is null)
        {
            customer = new Customer
            {
                Name = request.CustomerName,
                Email = request.CustomerEmail
            };
            _db.Customers.Add(customer);
        }

        var booking = new Booking
        {
            Customer = customer,
            ProviderId = slot.ProviderId,
            ServiceId = service.Id,
            AvailabilitySlotId = slot.Id,
            Status = BookingStatus.Confirmed
        };

        slot.IsBooked = true;

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        // Reload with related data for a clean response.
        await _db.Entry(booking).Reference(b => b.Provider).LoadAsync();
        await _db.Entry(booking).Reference(b => b.Service).LoadAsync();
        await _db.Entry(booking).Reference(b => b.AvailabilitySlot).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, ToResponse(booking));
    }

    // DELETE /api/bookings/5
    // Cancels the booking and frees up the slot again.
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel(int id)
    {
        var booking = await _db.Bookings
            .Include(b => b.AvailabilitySlot)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking is null)
        {
            return NotFound();
        }

        booking.Status = BookingStatus.Cancelled;

        if (booking.AvailabilitySlot is not null)
        {
            booking.AvailabilitySlot.IsBooked = false;
        }

        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static BookingResponse ToResponse(Booking booking) => new()
    {
        Id = booking.Id,
        CustomerName = booking.Customer?.Name ?? string.Empty,
        ProviderName = booking.Provider?.Name ?? string.Empty,
        ServiceName = booking.Service?.Name ?? string.Empty,
        StartTime = booking.AvailabilitySlot?.StartTime ?? default,
        EndTime = booking.AvailabilitySlot?.EndTime ?? default,
        Status = booking.Status.ToString()
    };
}
