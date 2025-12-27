using System.Globalization;
using System.Net.Http.Json;
using FishingLogApi.Models;

namespace FishingLogApi.Services;

public sealed class WeatherHistoryService
{
    private static readonly string[] WindDirections =
    [
        "N",
        "NE",
        "E",
        "SE",
        "S",
        "SW",
        "W",
        "NW"
    ];

    private readonly HttpClient _httpClient;

    public WeatherHistoryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<WeatherHistoryResponse> GetWeatherHistoryAsync(
        int year,
        double latitude,
        double longitude,
        CancellationToken cancellationToken)
    {
        var startDate = new DateOnly(year, 1, 1);
        var endDate = new DateOnly(year, 12, 31);

        var requestUrl =
            "https://archive-api.open-meteo.com/v1/archive" +
            $"?latitude={latitude.ToString(CultureInfo.InvariantCulture)}" +
            $"&longitude={longitude.ToString(CultureInfo.InvariantCulture)}" +
            $"&start_date={startDate:yyyy-MM-dd}" +
            $"&end_date={endDate:yyyy-MM-dd}" +
            "&daily=weathercode,wind_speed_10m_max,wind_direction_10m_dominant,relative_humidity_2m_mean,surface_pressure_mean" +
            "&timezone=auto";

        using var response = await _httpClient.GetAsync(requestUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<OpenMeteoArchiveResponse>(cancellationToken: cancellationToken);
        if (payload?.Daily?.Time == null)
        {
            throw new InvalidOperationException("Weather history response is missing daily data.");
        }

        var daily = payload.Daily;
        var monthLookup = new Dictionary<int, List<WeatherDayDto>>();

        for (var i = 0; i < daily.Time.Count; i++)
        {
            var date = DateOnly.Parse(daily.Time[i], CultureInfo.InvariantCulture);
            if (date.Month is < 4 or > 10)
            {
                continue;
            }

            var condition = MapCondition(daily.WeatherCode, i);
            var windSpeed = GetValueOrDefault(daily.WindSpeedMax, i);
            var windDirection = MapWindDirection(GetValueOrDefault(daily.WindDirectionDominant, i));
            var humidity = GetValueOrDefault(daily.RelativeHumidityMean, i);
            var pressure = GetValueOrDefault(daily.SurfacePressureMean, i);

            if (!monthLookup.TryGetValue(date.Month, out var monthDays))
            {
                monthDays = new List<WeatherDayDto>();
                monthLookup[date.Month] = monthDays;
            }

            monthDays.Add(new WeatherDayDto
            {
                Date = date,
                Condition = condition,
                WindSpeedMax = windSpeed,
                WindDirection = windDirection,
                RelativeHumidityMean = humidity,
                SurfacePressureMean = pressure
            });
        }

        var months = monthLookup
            .OrderBy(entry => entry.Key)
            .Select(entry => new WeatherMonthDto
            {
                Month = entry.Key,
                MonthName = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(entry.Key),
                Days = entry.Value
            })
            .ToList();

        return new WeatherHistoryResponse
        {
            Year = year,
            Latitude = latitude,
            Longitude = longitude,
            Months = months
        };
    }

    private static double? GetValueOrDefault(IReadOnlyList<double>? values, int index)
    {
        if (values == null || index < 0 || index >= values.Count)
        {
            return null;
        }

        return values[index];
    }

    private static string MapCondition(IReadOnlyList<int>? codes, int index)
    {
        if (codes == null || index < 0 || index >= codes.Count)
        {
            return "Unknown";
        }

        var code = codes[index];
        if (code == 0)
        {
            return "Sunny";
        }

        if (code is 1 or 2 or 3 or 45 or 48)
        {
            return "Cloudy";
        }

        if (code is >= 51 and <= 67 or >= 80 and <= 82 or >= 95 and <= 99)
        {
            return "Rain";
        }

        return "Cloudy";
    }

    private static string? MapWindDirection(double? degrees)
    {
        if (!degrees.HasValue)
        {
            return null;
        }

        var normalized = degrees.Value % 360;
        if (normalized < 0)
        {
            normalized += 360;
        }

        var index = (int)Math.Round(normalized / 45.0, MidpointRounding.AwayFromZero) % 8;
        return WindDirections[index];
    }
}
