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

        public static readonly GeoLocation Seydisfjordur =
            new(
                Name: "Seyðisfjörður",
                Latitude: 65.2630,
                Longitude: -14.0080,
                TimeZoneId: "Atlantic/Reykjavik"
            );

        public static readonly GeoLocation Raufarhofn =
            new(
                Name: "Raufarhöfn",
                Latitude: 66.4542,
                Longitude: -15.9434,
                TimeZoneId: "Atlantic/Reykjavik"
            );

        public static readonly GeoLocation Borgarnes =
            new(
                Name: "Borgarnes",
                Latitude: 64.5383,
                Longitude: -21.9202,
                TimeZoneId: "Atlantic/Reykjavik"
            );

        public static readonly GeoLocation Isafjordur =
            new(
                Name: "Ísafjörður",
                Latitude: 66.0749,
                Longitude: -23.1268,
                TimeZoneId: "Atlantic/Reykjavik"
            );

        public static readonly GeoLocation VikIMyrdal =
            new(
                Name: "Vík í mýrdal",
                Latitude: 63.4189,
                Longitude: -19.0064,
                TimeZoneId: "Atlantic/Reykjavik"
            );

        public static readonly GeoLocation Klaustur =
            new(
                Name: "Klaustur",
                Latitude: 63.7833,
                Longitude: -18.0583,
                TimeZoneId: "Atlantic/Reykjavik"
            );

        public static readonly GeoLocation Egilsstadir =
            new(
                Name: "Egilsstaðir",
                Latitude: 65.2669,
                Longitude: -14.3948,
                TimeZoneId: "Atlantic/Reykjavik"
            );

        public static readonly GeoLocation Blondous =
            new(
                Name: "Blönduós",
                Latitude: 65.6590,
                Longitude: -20.2850,
                TimeZoneId: "Atlantic/Reykjavik"
            );

        public static readonly GeoLocation Akureyri =
            new(
                Name: "Akureyri",
                Latitude: 65.6885,
                Longitude: -18.1262,
                TimeZoneId: "Atlantic/Reykjavik"
            );

        public static readonly GeoLocation Budardalur =
            new(
                Name: "Búðardalur",
                Latitude: 65.1100,
                Longitude: -21.7670,
                TimeZoneId: "Atlantic/Reykjavik"
            );
    }
}
