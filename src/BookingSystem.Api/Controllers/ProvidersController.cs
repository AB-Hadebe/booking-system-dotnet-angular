using BookingSystem.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProvidersController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProvidersController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/providers
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var providers = await _db.Providers
            .Select(p => new { p.Id, p.Name, p.Specialty })
            .ToListAsync();

        return Ok(providers);
    }
}
