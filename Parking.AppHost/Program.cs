using Microsoft.EntityFrameworkCore;
using Parking.Application.Contracts.Interfaces;
using Parking.Application.UseCases.ProcessPassage;
using Parking.Infrastructure.Persistence;
using Parking.Infrastructure.Persistence.Repositories;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using System;

var builder = WebApplication.CreateBuilder(args);

// =======================
// Infrastructure (EF)
// =======================
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
       .UseSnakeCaseNamingConvention());

// Repositories
builder.Services.AddScoped<IPassageRepository, PassageRepository>();
builder.Services.AddScoped<IParkingSessionRepository, ParkingSessionRepository>();
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();

// =======================
// Application
// =======================
builder.Services.AddScoped<ProcessPassageHandler>();
builder.Services.AddControllers();

var app = builder.Build();

// =======================
// Ingest endpoint
// =======================
app.MapPost("/ingest/passage", async (
    IngestPassageDto dto,
    HttpContext http,
    ProcessPassageHandler handler,
    IConfiguration cfg,
    CancellationToken ct) =>
{
    var apiKey = http.Request.Headers["X-Api-Key"].ToString();
    if (apiKey != cfg["Ingest:ApiKey"])
        return Results.Unauthorized();

    var cmd = new ProcessPassageCommand(
        OccurredAt: dto.OccurredAt,
        PlateRaw: dto.PlateRaw,
        Direction: dto.Direction,
        Confidence: dto.Confidence,
        JpegPath: dto.JpegPath
    );

    await handler.Handle(cmd, ct);
    return Results.Ok();
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapControllers();
app.Run();

// =======================
// DTO
// =======================
public sealed record IngestPassageDto(
    DateTimeOffset OccurredAt,
    string PlateRaw,
    Parking.Domain.ValueObjects.CameraDirection Direction,
    short? Confidence,
    string? JpegPath
);
