using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Parking.Infrastructure.Persistence; // <-- проверь namespace AppDbContext

namespace Parking.Migrations;

public static class Program
{
    // EF tools ищет именно этот метод
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((ctx, cfg) =>
            {
                // На всякий случай — чтобы appsettings.json точно подхватывался
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

                services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(cs));
            });

    // Обычный запуск нам не нужен, но пусть будет корректный entry point
    public static void Main(string[] args)
    {
        // Для миграций запускать проект не надо — EF вызовет CreateHostBuilder сам.
        Console.WriteLine("Parking.Migrations host");
    }
}
