using System.IO;
using FishingNewsWebScraper.Infrastructure;
using FishingNewsWebScraper.Options;
using FishingNewsWebScraper.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

var builder = Host.CreateApplicationBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    var logDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");
    Directory.CreateDirectory(logDirectory);

    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .WriteTo.Console()
        .WriteTo.File(
            Path.Combine(logDirectory, "fishingnews.log"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 14,
            shared: true);
});

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.Configure<ScraperOptions>(builder.Configuration.GetSection("Scraper"));
builder.Services.Configure<WeatherOptions>(builder.Configuration.GetSection("Weather"));
builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection("Database"));

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IFishingNewsRepository, SqlFishingNewsRepository>();
builder.Services.AddSingleton<IImageDownloader, ImageDownloader>();
builder.Services.AddSingleton<IWeatherObservationService, OpenMeteoWeatherObservationService>();
builder.Services.AddSingleton<IFishingNewsScraper, FishingNewsScraper>();
builder.Services.AddHostedService<ScraperWorker>();

var host = builder.Build();

await host.RunAsync();
