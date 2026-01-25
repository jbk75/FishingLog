using FishingLogApi.DAL.Models;
using FishingLogApi.DAL.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FishingLogApi.Controllers;

[Produces("application/json")]
[Route("api/fishingmap")]
public class FishingMapController : ControllerBase
{
    private readonly TripRepository _tripRepository;

    public FishingMapController(TripRepository tripRepository)
    {
        _tripRepository = tripRepository;
    }

    [HttpGet("trips")]
    public ActionResult<IEnumerable<TripMapDto>> GetTripMapData()
    {
        try
        {
            var trips = _tripRepository.GetTripMapData();
            return Ok(trips);
        }
        catch (Exception ex)
        {
            DAL.Logger.Logg("Error in GetTripMapData: " + ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }
}
