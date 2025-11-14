using FishingNewsWebScraper.Models;

namespace FishingNewsWebScraper.Infrastructure;

public interface IFishingNewsRepository
{
    Task<int> EnsureFishingPlaceAsync(FishingPlaceDetails place, CancellationToken cancellationToken);
    Task<int> InsertFishingNewsAsync(FishingNewsRecord record, CancellationToken cancellationToken);
    Task UpsertWeatherDetailsAsync(int fishingNewsId, IReadOnlyCollection<FishingNewsWeatherDetail> weatherDetails, CancellationToken cancellationToken);
    Task UpsertImagesAsync(int fishingNewsId, IReadOnlyCollection<FishingNewsImage> images, CancellationToken cancellationToken);
}
