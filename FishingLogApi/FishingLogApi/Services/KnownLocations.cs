using FishingLogApi.Models;

namespace FishingLogApi.Services
{
    public static class KnownLocations
    {
        public static readonly GeoLocation Reykjavik =
            new(
                Name: "Reykjavík",
                Latitude: 64.1466,
                Longitude: -21.9426,
                TimeZoneId: "Atlantic/Reykjavik"
            );
    }
}
