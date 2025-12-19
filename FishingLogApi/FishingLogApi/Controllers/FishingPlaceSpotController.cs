using FishingLogApi.DAL.Models;
using FishingLogApi.DAL.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FishingLogApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FishingPlaceSpotController : ControllerBase
{
    private readonly FishingPlaceSpotRepository _repository;
    private readonly ILogger<FishingPlaceSpotController> _logger;

    public FishingPlaceSpotController(FishingPlaceSpotRepository repository, ILogger<FishingPlaceSpotController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<IEnumerable<FishingPlaceSpotDto>> Get([FromQuery] int? fishingPlaceId)
    {
        _logger.LogInformation("Fetching fishing place spots with filter {FishingPlaceId}", fishingPlaceId);
        List<FishingPlaceSpotDto> spots = _repository.GetSpots(fishingPlaceId);
        return Ok(spots);
    }

    [HttpGet("exists")]
    public ActionResult<bool> Exists([FromQuery] int fishingPlaceId, [FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("Name is required.");
        }

        bool exists = _repository.SpotExists(fishingPlaceId, name);
        return Ok(exists);
    }

    [HttpPost]
    public ActionResult<int> Post([FromBody] CreateFishingPlaceSpotRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Name is required.");
        }

        if (!_repository.FishingPlaceExists(request.FishingPlaceId))
        {
            return BadRequest("Fishing place not found.");
        }

        if (_repository.SpotExists(request.FishingPlaceId, request.Name))
        {
            return Conflict($"Fishing spot '{request.Name}' already exists for this fishing place.");
        }

        int newId = _repository.AddSpot(request);
        _logger.LogInformation("Created fishing place spot {SpotName} for place {FishingPlaceId} with id {Id}", request.Name, request.FishingPlaceId, newId);
        return Ok(newId);
    }
}
