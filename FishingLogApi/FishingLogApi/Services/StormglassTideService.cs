using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using FishingLogApi.Models;
using Microsoft.Extensions.Options;

namespace FishingLogApi.Services;

public sealed class StormglassTideService
{
    private const int ChunkDays = 7;
    private readonly HttpClient _httpClient;
    private readonly StormglassOptions _options;
    private readonly ILogger<StormglassTideService> _logger;

    public StormglassTideService(
        HttpClient httpClient,
        IOptions<StormglassOptions> options,
        ILogger<StormglassTideService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TideEventDto>> GetTideEventsAsync(
        int year,
        double latitude,
        double longitude,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("Stormglass API key is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            throw new InvalidOperationException("Stormglass base URL is not configured.");
        }

        var startEpoch = new DateTimeOffset(year, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds();
        var endEpoch = new DateTimeOffset(year + 1, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds() - 1;
        var chunkSeconds = (long)TimeSpan.FromDays(ChunkDays).TotalSeconds;

        var events = new List<TideEventDto>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var currentStart = startEpoch;

        while (currentStart <= endEpoch)
        {
            var currentEnd = Math.Min(currentStart + chunkSeconds - 1, endEpoch);
            var url = BuildUrl(currentStart, currentEnd, latitude, longitude);

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", _options.ApiKey);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<StormglassTideResponse>(cancellationToken: cancellationToken);
            if (payload?.Data != null)
            {
                foreach (var entry in payload.Data)
                {
                    if (string.IsNullOrWhiteSpace(entry.Time) || string.IsNullOrWhiteSpace(entry.Type))
                    {
                        continue;
                    }

                    var key = $"{entry.Time}-{entry.Type}";
                    if (!seen.Add(key))
                    {
                        continue;
                    }

                    if (!DateTimeOffset.TryParse(entry.Time, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var timestamp))
                    {
                        _logger.LogWarning("Unable to parse Stormglass tide time {Time}.", entry.Time);
                        continue;
                    }

                    var level = string.Equals(entry.Type, "high", StringComparison.OrdinalIgnoreCase) ? "High" : "Low";
                    events.Add(new TideEventDto(timestamp, level));
                }
            }

            currentStart = currentEnd + 1;
        }

        return events;
    }

    private string BuildUrl(long startEpoch, long endEpoch, double latitude, double longitude)
    {
        return $"{_options.BaseUrl}?lat={latitude.ToString(CultureInfo.InvariantCulture)}" +
               $"&lng={longitude.ToString(CultureInfo.InvariantCulture)}" +
               $"&start={startEpoch}" +
               $"&end={endEpoch}";
    }

    private sealed class StormglassTideResponse
    {
        [JsonPropertyName("data")]
        public List<StormglassTideEntry>? Data { get; init; }
    }

    private sealed class StormglassTideEntry
    {
        [JsonPropertyName("time")]
        public string? Time { get; init; }

        [JsonPropertyName("type")]
        public string? Type { get; init; }
    }
}
