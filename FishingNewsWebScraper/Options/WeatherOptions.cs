namespace FishingNewsWebScraper.Options;

public sealed class WeatherOptions
{
    /// <summary>
    /// API endpoint template for the weather provider. The template should include placeholders {latitude}, {longitude} and {date} (yyyy-MM-dd).
    /// Example: https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&start_date={date}&end_date={date}&hourly=temperature_2m,precipitation,weathercode,windspeed_10m
    /// </summary>
    public string ObservationEndpointTemplate { get; set; } = string.Empty;

    /// <summary>
    /// Optional API endpoint template for tide information. The template should include placeholders {latitude}, {longitude} and {date}.
    /// </summary>
    public string? TideEndpointTemplate { get; set; }
}
