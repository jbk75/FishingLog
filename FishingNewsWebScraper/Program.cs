using System.IO;
using FishingNewsWebScraper.Infrastructure;
using FishingNewsWebScraper.Options;
using FishingNewsWebScraper.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
    })
    .UseSerilog((context, services, configuration) =>
    {
        var logDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");
        Directory.CreateDirectory(logDirectory);

        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .WriteTo.Sink(new ConsoleLogEventSink())
            .WriteTo.File(
                Path.Combine(logDirectory, "fishingnews.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                shared: true);
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<ScraperOptions>(context.Configuration.GetSection("Scraper"));
        services.Configure<WeatherOptions>(context.Configuration.GetSection("Weather"));
        services.Configure<DatabaseOptions>(context.Configuration.GetSection("Database"));

        services.AddHttpClient();
        services.AddSingleton<IFishingNewsRepository, SqlFishingNewsRepository>();
        services.AddSingleton<IImageDownloader, ImageDownloader>();
        services.AddSingleton<IWeatherObservationService, OpenMeteoWeatherObservationService>();
        services.AddSingleton<IFishingNewsScraper, FishingNewsScraper>();
        services.AddHostedService<ScraperWorker>();
    })
    .Build();

await host.RunAsync();
