using System.IO;
using System.Linq;
using FishingNewsWebScraper.Models;
using FishingNewsWebScraper.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FishingNewsWebScraper.Services;

public sealed class ImageDownloader : IImageDownloader
{
    private readonly ILogger<ImageDownloader> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ScraperOptions _options;

    public ImageDownloader(ILogger<ImageDownloader> logger, IHttpClientFactory httpClientFactory, IOptions<ScraperOptions> options)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public async Task DownloadAsync(int newsId, FishingNewsRecord record, CancellationToken cancellationToken)
    {
        if (record.Images.Count == 0)
        {
            _logger.LogInformation("Skipping image download for news {NewsId} because there are no images.", newsId);
            return;
        }

        var basePath = Path.IsPathRooted(_options.ImageDirectory)
            ? _options.ImageDirectory
            : Path.Combine(AppContext.BaseDirectory, _options.ImageDirectory);

        Directory.CreateDirectory(basePath);

        var client = _httpClientFactory.CreateClient();

        _logger.LogInformation(
            "Downloading {ImageCount} images for news {NewsId} into {Directory}.",
            record.Images.Count,
            newsId,
            basePath);

        for (var index = 0; index < record.Images.Count; index++)
        {
            var image = record.Images[index];
            try
            {
                await using var responseStream = await client.GetStreamAsync(image.SourceUri, cancellationToken);
                var fileName = $"{record.Date:yyyyMMdd}_{Normalize(record.FishingPlace.Name)}_{newsId:D6}_{index + 1}.jpg";
                var path = Path.Combine(basePath, fileName);
                await using var fileStream = File.Create(path);
                await responseStream.CopyToAsync(fileStream, cancellationToken);
                image.LocalPath = path;
                _logger.LogInformation(
                    "Downloaded image {Index}/{Total} for news {NewsId} from {Uri} to {Path}.",
                    index + 1,
                    record.Images.Count,
                    newsId,
                    image.SourceUri,
                    path);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to download image {Uri} for fishing news {NewsId}.", image.SourceUri, newsId);
            }
        }
    }

    private static string Normalize(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(value.Select(ch => invalidChars.Contains(ch) ? '-' : ch).ToArray());
        return sanitized.Replace(' ', '_').ToLowerInvariant();
    }
}
