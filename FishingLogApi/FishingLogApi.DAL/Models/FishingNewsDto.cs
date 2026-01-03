namespace FishingLogApi.DAL.Models;

public sealed class FishingNewsDto
{
    public int Id { get; set; }
    public int FishingPlaceId { get; set; }
    public string? FishingPlaceName { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
}
