using FishingNewsWebScraper.Infrastructure;
using FishingNewsWebScraper.Models;
using FishingNewsWebScraper.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FishingNewsWebScraper.Services;

public sealed class ScraperWorker : BackgroundService
{
    private readonly ILogger<ScraperWorker> _logger;
    private readonly IFishingNewsScraper _scraper;
    private readonly IFishingNewsRepository _repository;
    private readonly IImageDownloader _imageDownloader;
    private readonly IWeatherObservationService _weatherService;
    private readonly ScraperOptions _options;

    public ScraperWorker(
        ILogger<ScraperWorker> logger,
        IFishingNewsScraper scraper,
        IFishingNewsRepository repository,
        IImageDownloader imageDownloader,
        IWeatherObservationService weatherService,
        IOptions<ScraperOptions> options)
    {
        _logger = logger;
        _scraper = scraper;
        _repository = repository;
        _imageDownloader = imageDownloader;
        _weatherService = weatherService;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting fishing news scraping job. Going {YearsBack} years back.", _options.YearsBack);

        var fromDate = DateTime.UtcNow.Date.AddYears(-Math.Abs(_options.YearsBack));
        IReadOnlyList<FishingNewsRecord> newsItems;

        try
        {
            newsItems = await _scraper.ScrapeAsync(fromDate, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to scrape fishing news.");
            return;
        }

        foreach (var record in newsItems)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                var placeId = await _repository.EnsureFishingPlaceAsync(record.FishingPlace, stoppingToken);
                record.FishingPlaceId = placeId;

                if (!record.WeatherDetails.Any())
                {
                    var enrichedWeather = await _weatherService.GetWeatherDetailsAsync(record, stoppingToken);
                    if (enrichedWeather.Count > 0)
                    {
                        record.WeatherDetails.AddRange(enrichedWeather);
                    }
                }

                var newsId = await _repository.InsertFishingNewsAsync(record, stoppingToken);

                if (record.WeatherDetails.Count > 0)
                {
                    await _repository.UpsertWeatherDetailsAsync(newsId, record.WeatherDetails, stoppingToken);
                }

                if (record.Images.Count > 0)
                {
                    await _imageDownloader.DownloadAsync(newsId, record, stoppingToken);
                    await _repository.UpsertImagesAsync(newsId, record.Images, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist fishing news item for {Place} on {Date}.", record.FishingPlace.Name, record.Date);
            }
        }

        _logger.LogInformation("Fishing news scraping job completed.");
    }
}
