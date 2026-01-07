namespace FishingLogApi.Models;

public sealed record TideEventsResponse(
    int Year,
    double Latitude,
    double Longitude,
    string Model,
    IReadOnlyList<TideEventDto> Events);
