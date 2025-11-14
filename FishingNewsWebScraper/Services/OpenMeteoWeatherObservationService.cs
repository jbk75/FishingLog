using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using FishingNewsWebScraper.Models;
using FishingNewsWebScraper.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FishingNewsWebScraper.Services;

public sealed class OpenMeteoWeatherObservationService : IWeatherObservationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WeatherOptions _options;
    private readonly ILogger<OpenMeteoWeatherObservationService> _logger;

    public OpenMeteoWeatherObservationService(
        IHttpClientFactory httpClientFactory,
        IOptions<WeatherOptions> options,
        ILogger<OpenMeteoWeatherObservationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<FishingNewsWeatherDetail>> GetWeatherDetailsAsync(FishingNewsRecord record, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ObservationEndpointTemplate))
        {
            return Array.Empty<FishingNewsWeatherDetail>();
        }

        if (string.IsNullOrWhiteSpace(record.FishingPlace.Latitude) || string.IsNullOrWhiteSpace(record.FishingPlace.Longitude))
        {
            _logger.LogDebug("Skipping weather lookup because the fishing place {Place} is missing coordinates.", record.FishingPlace.Name);
            return Array.Empty<FishingNewsWeatherDetail>();
        }

        var client = _httpClientFactory.CreateClient();
        var date = record.Date.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var latitude = record.FishingPlace.Latitude;
        var longitude = record.FishingPlace.Longitude;
        var observationUrl = _options.ObservationEndpointTemplate
            .Replace("{latitude}", latitude, StringComparison.OrdinalIgnoreCase)
            .Replace("{longitude}", longitude, StringComparison.OrdinalIgnoreCase)
            .Replace("{date}", date, StringComparison.OrdinalIgnoreCase);

        try
        {
            using var response = await client.GetAsync(observationUrl, cancellationToken);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var details = ParseWeather(document, record);

            if (!string.IsNullOrWhiteSpace(_options.TideEndpointTemplate))
            {
                var tideUrl = _options.TideEndpointTemplate
                    .Replace("{latitude}", latitude, StringComparison.OrdinalIgnoreCase)
                    .Replace("{longitude}", longitude, StringComparison.OrdinalIgnoreCase)
                    .Replace("{date}", date, StringComparison.OrdinalIgnoreCase);

                var tideDetails = await FetchTideStateAsync(client, tideUrl, cancellationToken);
                foreach (var detail in details)
                {
                    detail.TideState ??= tideDetails;
                }

                if (!string.IsNullOrEmpty(tideDetails))
                {
                    record.TideState = tideDetails;
                }
            }

            return details;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch weather data for {Place} using url {Url}.", record.FishingPlace.Name, observationUrl);
            return Array.Empty<FishingNewsWeatherDetail>();
        }
    }

    private IReadOnlyList<FishingNewsWeatherDetail> ParseWeather(JsonDocument document, FishingNewsRecord record)
    {
        if (!document.RootElement.TryGetProperty("hourly", out var hourly))
        {
            return Array.Empty<FishingNewsWeatherDetail>();
        }

        if (!hourly.TryGetProperty("time", out var times) || times.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<FishingNewsWeatherDetail>();
        }

        var temperatures = hourly.TryGetProperty("temperature_2m", out var tempElement) && tempElement.ValueKind == JsonValueKind.Array
            ? tempElement.EnumerateArray().Select(e => e.TryGetDecimal(out var value) ? value : (decimal?)null).ToArray()
            : Array.Empty<decimal?>();

        var precipitations = hourly.TryGetProperty("precipitation", out var precipitationElement) && precipitationElement.ValueKind == JsonValueKind.Array
            ? precipitationElement.EnumerateArray().Select(e => e.TryGetDecimal(out var value) ? value : (decimal?)null).ToArray()
            : Array.Empty<decimal?>();

        var windSpeeds = hourly.TryGetProperty("windspeed_10m", out var windElement) && windElement.ValueKind == JsonValueKind.Array
            ? windElement.EnumerateArray().Select(e => e.TryGetDecimal(out var value) ? value : (decimal?)null).ToArray()
            : Array.Empty<decimal?>();

        var windDirections = hourly.TryGetProperty("winddirection_10m", out var windDirectionElement) && windDirectionElement.ValueKind == JsonValueKind.Array
            ? windDirectionElement.EnumerateArray().Select(e => e.TryGetDecimal(out var value) ? value : (decimal?)null).ToArray()
            : Array.Empty<decimal?>();

        var weatherCodes = hourly.TryGetProperty("weathercode", out var weatherCodeElement) && weatherCodeElement.ValueKind == JsonValueKind.Array
            ? weatherCodeElement.EnumerateArray().Select(e => e.TryGetInt32(out var value) ? value : (int?)null).ToArray()
            : Array.Empty<int?>();

        var details = new List<FishingNewsWeatherDetail>();
        var index = 0;
        foreach (var timeElement in times.EnumerateArray())
        {
            if (timeElement.ValueKind != JsonValueKind.String)
            {
                index++;
                continue;
            }

            if (!DateTimeOffset.TryParse(timeElement.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var timestamp))
            {
                index++;
                continue;
            }

            if (timestamp.Date != record.Date.ToDateTime(TimeOnly.MinValue).Date)
            {
                index++;
                continue;
            }

            details.Add(new FishingNewsWeatherDetail
            {
                ObservationTime = timestamp,
                TemperatureC = index < temperatures.Length ? temperatures[index] : null,
                PrecipitationMillimeters = index < precipitations.Length ? precipitations[index] : null,
                WindSpeedMetersPerSecond = index < windSpeeds.Length ? windSpeeds[index] : null,
                WindDirection = index < windDirections.Length ? DescribeWindDirection(windDirections[index]) : null,
                Summary = index < weatherCodes.Length ? DescribeWeatherCode(weatherCodes[index]) : "Weather observation"
            });

            index++;
        }

        return details;
    }

    private async Task<string?> FetchTideStateAsync(HttpClient client, string url, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await client.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return content.Length > 200 ? content[..200] : content;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to fetch tide information from {Url}.", url);
            return null;
        }
    }

    private static string? DescribeWindDirection(decimal? degrees)
    {
        if (degrees is null)
        {
            return null;
        }

        var normalized = (double)degrees.Value;
        normalized %= 360d;
        if (normalized < 0)
        {
            normalized += 360d;
        }

        string Cardinal(int index) => index switch
        {
            0 => "N",
            1 => "NE",
            2 => "E",
            3 => "SE",
            4 => "S",
            5 => "SW",
            6 => "W",
            _ => "NW"
        };

        var bucket = (int)Math.Round(normalized / 45d) % 8;
        return $"{Cardinal(bucket)} ({normalized:F0}°)";
    }

    private static string DescribeWeatherCode(int? code)
    {
        return code switch
        {
            0 => "Clear sky",
            1 or 2 => "Partly cloudy",
            3 => "Overcast",
            45 or 48 => "Fog",
            51 or 53 or 55 => "Drizzle",
            56 or 57 => "Freezing drizzle",
            61 or 63 or 65 => "Rain",
            66 or 67 => "Freezing rain",
            71 or 73 or 75 => "Snow",
            77 => "Snow grains",
            80 or 81 or 82 => "Rain showers",
            85 or 86 => "Snow showers",
            95 => "Thunderstorm",
            96 or 99 => "Thunderstorm with hail",
            _ => "Weather observation"
        };
    }
}
