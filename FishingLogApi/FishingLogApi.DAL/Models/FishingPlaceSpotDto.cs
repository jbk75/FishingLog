namespace FishingLogApi.DAL.Models;

public class FishingPlaceSpotDto
{
    public int Id { get; set; }
    public int FishingPlaceId { get; set; }
    public string FishingPlaceName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime LastModified { get; set; }
}

public class CreateFishingPlaceSpotRequest
{
    public int FishingPlaceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateFishingPlaceSpotDescriptionRequest
{
    public string? Description { get; set; }
}
