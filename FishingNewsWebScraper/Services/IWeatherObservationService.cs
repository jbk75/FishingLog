using FishingNewsWebScraper.Models;

namespace FishingNewsWebScraper.Services;

public interface IWeatherObservationService
{
    Task<IReadOnlyList<FishingNewsWeatherDetail>> GetWeatherDetailsAsync(FishingNewsRecord record, CancellationToken cancellationToken);
}
