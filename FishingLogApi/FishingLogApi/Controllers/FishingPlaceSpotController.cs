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
    public ActionResult<IEnumerable<FishingPlaceSpotDto>> GetAll()
    {
        _logger.LogInformation("Fetching all fishing place spots");
        List<FishingPlaceSpotDto> spots = _repository.GetSpots();
        return Ok(spots);
    }

    [HttpGet("{id:int}")]
    public ActionResult<FishingPlaceSpotDto> GetById([FromRoute] int id)
    {
        _logger.LogInformation("Fetching fishing place spot {SpotId}", id);
        FishingPlaceSpotDto? spot = _repository.GetSpotById(id);
        if (spot == null)
        {
            return NotFound();
        }

        return Ok(spot);
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

    [HttpPut("{id:int}/description")]
    public ActionResult<FishingPlaceSpotDto> UpdateDescription([FromRoute] int id, [FromBody] UpdateFishingPlaceSpotDescriptionRequest request)
    {
        if (request == null)
        {
            return BadRequest("Description payload is required.");
        }

        bool updated = _repository.UpdateSpotDescription(id, request.Description);
        if (!updated)
        {
            return NotFound();
        }

        FishingPlaceSpotDto? spot = _repository.GetSpotById(id);
        if (spot == null)
        {
            return NotFound();
        }

        _logger.LogInformation("Updated description for fishing place spot {SpotId}", id);
        return Ok(spot);
    }
}
