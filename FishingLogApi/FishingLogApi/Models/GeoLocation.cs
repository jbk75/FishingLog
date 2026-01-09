namespace FishingLogApi.Models
{
public sealed record GeoLocation(
    string Name,
    double Latitude,
    double Longitude,
    string TimeZoneId // e.g. "Atlantic/Reykjavik"
);

}
