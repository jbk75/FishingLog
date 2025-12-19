namespace FishingLogApi.DAL.Models;

public class FishingPlaceWishlistItem
{
    public int Id { get; set; }
    public int FishingPlaceId { get; set; }
    public string FishingPlaceName { get; set; } = string.Empty;
    public int FishingPlaceTypeId { get; set; }
    public string FishingPlaceTypeName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class FishingPlaceWishlistCreateRequest
{
    public int? FishingPlaceId { get; set; }
    public string? FishingPlaceName { get; set; }
    public int FishingPlaceTypeId { get; set; }
    public string? Description { get; set; }
}
