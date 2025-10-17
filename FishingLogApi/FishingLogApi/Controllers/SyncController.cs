using FishingLogApi.DAL.Models;
using FishingLogApi.DAL.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FishingLogApi.Controllers;

[ApiController]
[Route("api/sync")]
[Produces("application/json")]
public class TripSyncController : ControllerBase
{
    private readonly TripRepository _repository;

    public TripSyncController(TripRepository repository)
    {
        _repository = repository;
    }

    [HttpPost("trips")]
    public async Task<IActionResult> SyncTrips([FromBody] TripSyncRequest request)
    {
        // Apply client changes to SQL Server
        await _repository.UpsertTripsAsync(request.ClientTrips);

        // Fetch server changes since last sync
        var updatedTrips = await _repository.GetTripsChangedSinceAsync(request.LastSyncTimeUtc);

        return Ok(updatedTrips);
    }
}

public class TripSyncRequest
{
    public DateTime LastSyncTimeUtc { get; set; }
    public List<TripDto> ClientTrips { get; set; } = new();
}

