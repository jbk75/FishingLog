using Microsoft.AspNetCore.Mvc;
using FishingLogApi.DAL;
using FishingLogApi.DAL.Models;
using FishingLogApi.DAL.Repositories;
using Microsoft.Extensions.Logging;

namespace FishingLogApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class VeidistadurController : ControllerBase
{
    private readonly VeidistadurRepository _repository;
    private readonly ILogger<VeidistadurController> _logger;

    public VeidistadurController(VeidistadurRepository repository, ILogger<VeidistadurController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    // GET: api/Veidistadur
    [HttpGet]
    public ActionResult<IEnumerable<FishingPlace>> Get()
    {
        _logger.LogInformation("Veidistadir Get started");

        List<FishingPlace> result = _repository.GetVeidistadir();

        _logger.LogInformation("Veidistadir Get completed");

        return Ok(result);
    }

    // POST: api/Veidistadur
    [HttpPost]
    public ActionResult<int> Post([FromBody] FishingPlace model)
    {
        _logger.LogInformation("Veidistadur Post started");

        if (model == null || string.IsNullOrWhiteSpace(model.Name))
        {
            _logger.LogWarning("Invalid model in POST");
            return BadRequest("Heiti is required.");
        }

        try
        {
            int newId = _repository.AddVeidistadur(model);

            _logger.LogInformation($"Veidistadur '{model.Name}' created with ID {newId}");

            return Ok(newId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while adding Veidistadur");
            return StatusCode(500, "An error occurred while saving the veiðistaður.");
        }
    }

    [HttpGet("{id:int}/relations")]
    public ActionResult<FishingPlaceRelationCounts> GetRelations([FromRoute] int id)
    {
        _logger.LogInformation("Veidistadur relation check started for {FishingPlaceId}", id);

        FishingPlace? place = _repository.GetById(id);
        if (place == null)
        {
            return NotFound();
        }

        FishingPlaceRelationCounts counts = _repository.GetRelationCounts(id);
        return Ok(counts);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete([FromRoute] int id)
    {
        _logger.LogInformation("Veidistadur delete started for {FishingPlaceId}", id);

        FishingPlace? place = _repository.GetById(id);
        if (place == null)
        {
            return NotFound();
        }

        FishingPlaceRelationCounts counts = _repository.GetRelationCounts(id);
        if (counts.FishingNewsCount > 0 || counts.FishingPlaceSpotCount > 0 || counts.TripCount > 0)
        {
            return Conflict(counts);
        }

        bool deleted = _repository.DeleteVeidistadur(id);
        if (!deleted)
        {
            return StatusCode(500, "Unable to delete fishing place.");
        }

        return NoContent();
    }
}
