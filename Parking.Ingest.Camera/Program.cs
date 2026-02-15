using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Parking.Application.Contracts.Interfaces;
using Parking.Application.UseCases.ProcessPassage;
using Parking.Infrastructure;
using Parking.Infrastructure.Persistence;
using Parking.Infrastructure.Persistence.Repositories;
using Parking.Ingest.Camera;

Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.Configure<CameraOptions>(ctx.Configuration.GetSection("Camera"));
        services.Configure<IngestClientOptions>(ctx.Configuration.GetSection("IngestClient"));

        services.AddHttpClient<IngestApiClient>();

        services.AddSingleton<CameraClient>();
        services.AddHostedService<ParkingIngestCamera>();
    })
    .Build()
    .Run();
