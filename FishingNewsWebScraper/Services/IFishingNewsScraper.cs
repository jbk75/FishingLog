using FishingNewsWebScraper.Models;

namespace FishingNewsWebScraper.Services;

public interface IFishingNewsScraper
{
    Task<IReadOnlyList<FishingNewsRecord>> ScrapeAsync(DateTime fromDate, CancellationToken cancellationToken);
}
