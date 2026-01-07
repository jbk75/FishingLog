namespace FishingLogApi.Services;

public sealed class StormglassOptions
{
    public string BaseUrl { get; init; } = "https://api.stormglass.io/v2/tide/extremes/point";
    public string? ApiKey { get; init; }
}
