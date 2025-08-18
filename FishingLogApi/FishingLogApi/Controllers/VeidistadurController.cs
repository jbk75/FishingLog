using Microsoft.AspNetCore.Mvc;
using FishingLogApi.DAL;
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
            int newId = VeidistadurRepository.AddVeidistadur(model);

            _logger.LogInformation($"Veidistadur '{model.Name}' created with ID {newId}");

            return Ok(newId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while adding Veidistadur");
            return StatusCode(500, "An error occurred while saving the veiðistaður.");
        }
    }
}
