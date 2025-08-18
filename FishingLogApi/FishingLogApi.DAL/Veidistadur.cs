

namespace FishingLogApi.DAL;

public class FishingPlace
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int NumberOfSpots { get; set; }
    public int FishingPlaceTypeID { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Longitude { get; set; } = string.Empty;
    public string Latitude { get; set; } = string.Empty;

    //public string VeidileyfiSimanumer { get; set; } = string.Empty;
    //public string Veidileyfasali { get; set; } = string.Empty;
    //public string Vefsidur { get; set; } = string.Empty;
}
