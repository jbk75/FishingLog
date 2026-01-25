namespace FishingLogApi.DAL.Models;

public class TripMapDto
{
    public int Id { get; set; }
    public int FishingPlaceId { get; set; }
    public string? FishingPlaceName { get; set; }
    public string? Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public string? Longitude { get; set; }
    public string? Latitude { get; set; }
}
