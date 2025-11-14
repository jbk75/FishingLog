using FishingNewsWebScraper.Models;

namespace FishingNewsWebScraper.Services;

public interface IImageDownloader
{
    Task DownloadAsync(int newsId, FishingNewsRecord record, CancellationToken cancellationToken);
}
