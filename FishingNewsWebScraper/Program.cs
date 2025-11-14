using FishingNewsWebScraper.Infrastructure;
using FishingNewsWebScraper.Options;
using FishingNewsWebScraper.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

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

builder.Logging.AddConsole();

var host = builder.Build();

await host.RunAsync();
