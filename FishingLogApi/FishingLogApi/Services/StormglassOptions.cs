namespace FishingLogApi.Services;

public sealed class StormglassOptions
{
    public string BaseUrl { get; init; } = "https://api.stormglass.io/v2/tide/extremes/point";
    public string? ApiKey { get; init; } = "1a08bf46-ec0d-11f0-a0d3-0242ac130003-1a08bff0-ec0d-11f0-a0d3-0242ac130003";
}
