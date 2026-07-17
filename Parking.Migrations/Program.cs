using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Parking.Infrastructure.Persistence;

namespace Parking.Migrations;

public static class Program
{
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((ctx, cfg) =>
            {
                cfg.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
                cfg.AddEnvironmentVariables();
            })
            .ConfigureServices((ctx, services) =>
            {
                var cs =
                    ctx.Configuration.GetConnectionString("Default")
                    ?? Environment.GetEnvironmentVariable("PARKING_DB")
                    ?? throw new InvalidOperationException(
                        "No connection string. Set ConnectionStrings:Default in appsettings.json or PARKING_DB env var.");

                services.AddDbContext<AppDbContext>(opt =>
                    opt.UseNpgsql(cs)
                       .UseSnakeCaseNamingConvention());
            });

    public static void Main(string[] args)
    {
        Console.WriteLine("Parking.Migrations host");
    }
}
