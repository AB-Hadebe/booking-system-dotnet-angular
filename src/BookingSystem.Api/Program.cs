using BookingSystem.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EF Core - points at the "DefaultConnection" string in appsettings.json.
// Locally this is SQL Server/LocalDB; in Azure it becomes the Azure SQL connection string.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Allow the Angular dev server (ng serve, default port 4200) to call this API locally.
const string AngularDevCorsPolicy = "AngularDevCorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularDevCorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(AngularDevCorsPolicy);
}

// Skip the HTTPS redirect in development so the Angular dev server (http://localhost:4200)
// can call http://localhost:5017 without a cross-origin redirect getting in the way.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Development only: top up availability slots so there is always something to book.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await DbSeeder.SeedAvailabilityAsync(db);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Could not seed availability slots. Has the database been migrated?");
    }
}

app.UseAuthorization();

app.MapControllers();

app.Run();
