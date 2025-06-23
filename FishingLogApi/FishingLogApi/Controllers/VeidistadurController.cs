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
    public ActionResult<IEnumerable<Veidistadur>> Get()
    {
        _logger.LogInformation("Veidistadir Get started");

        var result = _repository.GetVeidistadir();

        _logger.LogInformation("Veidistadir Get completed");

        return Ok(result);
    }
}
