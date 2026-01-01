namespace FishingLogApi.DAL.Models;

public class TripDto
{
    public int? Id { get; set; }
    public string? Description { get; set; }
    public DateTime Timastimpill { get; set; } = DateTime.Now;
    public DateTime DagsFra { get; set; }
    public DateTime DagsTil { get; set; }
    public int VsId { get; set; }

    /// <summary>
    /// columns for syncing
    /// </summary>
    public Guid SyncId { get; set; }
    public DateTime LastModifiedUtc { get; set; }
    public bool IsDeleted { get; set; }

    /////// end of columns for syncing ///////////////////
}

public class TripSyncRequest
{
    public DateTime LastSyncTimeUtc { get; set; }
    public List<TripDto> ClientTrips { get; set; } = [];
}
