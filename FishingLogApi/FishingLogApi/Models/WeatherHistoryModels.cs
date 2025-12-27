using System.Text.Json.Serialization;

namespace FishingLogApi.Models;

public sealed class WeatherHistoryResponse
{
    public int Year { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public IReadOnlyList<WeatherMonthDto> Months { get; init; } = [];
}

public sealed class WeatherMonthDto
{
    public int Month { get; init; }
    public string MonthName { get; init; } = string.Empty;
    public IReadOnlyList<WeatherDayDto> Days { get; init; } = [];
}

public sealed class WeatherDayDto
{
    public DateOnly Date { get; init; }
    public string Condition { get; init; } = string.Empty;
    public double? WindSpeedMax { get; init; }
    public string? WindDirection { get; init; }
    public double? RelativeHumidityMean { get; init; }
    public double? SurfacePressureMean { get; init; }
}

public sealed class OpenMeteoArchiveResponse
{
    [JsonPropertyName("daily")]
    public OpenMeteoDaily? Daily { get; init; }
}

public sealed class OpenMeteoDaily
{
    [JsonPropertyName("time")]
    public List<string>? Time { get; init; }

    [JsonPropertyName("weathercode")]
    public List<int>? WeatherCode { get; init; }

    [JsonPropertyName("wind_speed_10m_max")]
    public List<double>? WindSpeedMax { get; init; }

    [JsonPropertyName("wind_direction_10m_dominant")]
    public List<double>? WindDirectionDominant { get; init; }

    [JsonPropertyName("relative_humidity_2m_mean")]
    public List<double>? RelativeHumidityMean { get; init; }

    [JsonPropertyName("surface_pressure_mean")]
    public List<double>? SurfacePressureMean { get; init; }
}
