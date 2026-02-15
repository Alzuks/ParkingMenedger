using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Parking.Application.UseCases.ProcessPassage;

namespace Parking.Ingest.Camera;

public sealed class ParkingIngestCamera : BackgroundService
{
    private readonly ILogger<ParkingIngestCamera> _log;
    private readonly CameraClient _camera;
    private readonly IServiceProvider _sp;

    public ParkingIngestCamera(
        ILogger<ParkingIngestCamera> log,
        CameraClient camera,
        IServiceProvider sp)
    {
        _log = log;
        _camera = camera;
        _sp = sp;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log.LogInformation("Parking.Ingest.Camera started");

        _camera.OnPassage += cmd =>
        {
            _ = HandlePassage(cmd, stoppingToken);
        };

        _camera.Start();

        // держим сервис живым
        return Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandlePassage(
    ProcessPassageCommand cmd,
    CancellationToken ct)
    {
        try
        {
            using var scope = _sp.CreateScope();

            var api = scope.ServiceProvider
                .GetRequiredService<IngestApiClient>();

            await api.SendAsync(cmd, ct);

            _log.LogInformation(
                "Passage sent: {Plate} {Dir} {Time}",
                cmd.PlateRaw,
                cmd.Direction,
                cmd.OccurredAt);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to send passage to API");
        }
    }


    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _log.LogInformation("Parking.Ingest.Camera stopping");
        _camera.Stop();
        return base.StopAsync(cancellationToken);
    }
}
