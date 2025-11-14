namespace FishingNewsWebScraper.Models;

public sealed class FishingNewsRecord
{
    public DateOnly Date { get; set; }
    public TimeOnly? Time { get; set; }
    public FishingPlaceDetails FishingPlace { get; set; } = new();
    public int? FishingPlaceId { get; set; }
    public int? NumberOfFishesCaught { get; set; }
    public int? NumberOfFishesSeen { get; set; }
    public List<string> FishSpecies { get; } = new();
    public List<FishingNewsWeatherDetail> WeatherDetails { get; } = new();
    public string Description { get; set; } = string.Empty;
    public string SourceOfNews { get; set; } = string.Empty;
    public List<FishingNewsImage> Images { get; } = new();
    public List<FishCatchDetail> CatchDetails { get; } = new();
    public string? TideState { get; set; }
    public TimeOnly? PeakActivityTime { get; set; }
}

public sealed class FishingPlaceDetails
{
    public string Name { get; set; } = string.Empty;
    public string? Longitude { get; set; }
    public string? Latitude { get; set; }
    public int? NumberOfSpots { get; set; }
    public string? Description { get; set; }
    public int? FishingPlaceTypeId { get; set; }
}

public sealed class FishCatchDetail
{
    public string Species { get; set; } = string.Empty;
    public decimal? WeightKg { get; set; }
    public decimal? LengthCm { get; set; }
    public bool? Released { get; set; }
}

public sealed class FishingNewsImage
{
    public Uri SourceUri { get; set; } = new("http://localhost");
    public string? Caption { get; set; }
    public string? LocalPath { get; set; }
}

public sealed class FishingNewsWeatherDetail
{
    public DateTimeOffset ObservationTime { get; set; }
    public string Summary { get; set; } = string.Empty;
    public decimal? TemperatureC { get; set; }
    public decimal? WindSpeedMetersPerSecond { get; set; }
    public decimal? PrecipitationMillimeters { get; set; }
    public string? TideState { get; set; }
    public string? WindDirection { get; set; }
}
