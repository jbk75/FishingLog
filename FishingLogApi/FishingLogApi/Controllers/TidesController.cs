using FishingLogApi.Models;
using FishingLogApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FishingLogApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TidesController : ControllerBase
{
    private readonly TideCalculator _calculator;
    private readonly ILogger<TidesController> _logger;

    public TidesController(TideCalculator calculator, ILogger<TidesController> logger)
    {
        _calculator = calculator;
        _logger = logger;
    }

    // GET: api/tides/{year}
    [HttpGet("{year:int}")]
    public ActionResult<YearlyTideSummary> Get(int year)
    {
        if (year < 1 || year > 9999)
        {
            return BadRequest("Year must be between 1 and 9999.");
        }

        _logger.LogInformation("Generating tide estimates for {Year}.", year);
        var summary = _calculator.CalculateYear(year);
        return Ok(summary);
    }
}
