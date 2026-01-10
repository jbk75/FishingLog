namespace FishingLogApi.Models;

public class SuggestedFishingTripDto
{
    public int PlaceId { get; set; }
    public string PlaceName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int TripCount { get; set; }
    public int NewsCount { get; set; }
}

public class SuggestedFishingTripsResponse
{
    public int SeasonYear { get; set; }
    public List<SuggestedFishingTripDto> Suggestions { get; set; } = [];
}
