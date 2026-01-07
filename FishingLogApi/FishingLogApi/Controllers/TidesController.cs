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
    private readonly StormglassTideService _stormglassService;
    private readonly ILogger<TidesController> _logger;

    public TidesController(
        TideCalculator calculator,
        StormglassTideService stormglassService,
        ILogger<TidesController> logger)
    {
        _calculator = calculator;
        _stormglassService = stormglassService;
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

    // GET: api/tides/stormglass/{year}?lat=...&lon=...
    [HttpGet("stormglass/{year:int}")]
    public async Task<ActionResult<TideEventsResponse>> GetStormglass(
        int year,
        [FromQuery] double lat,
        [FromQuery] double lon,
        CancellationToken cancellationToken)
    {
        if (year < 1 || year > 9999)
        {
            return BadRequest("Year must be between 1 and 9999.");
        }

        if (double.IsNaN(lat) || double.IsNaN(lon) || double.IsInfinity(lat) || double.IsInfinity(lon))
        {
            return BadRequest("Latitude and longitude must be valid numbers.");
        }

        _logger.LogInformation("Fetching Stormglass tide data for {Year} ({Lat}, {Lon}).", year, lat, lon);

        try
        {
            var events = await _stormglassService.GetTideEventsAsync(year, lat, lon, cancellationToken);
            var response = new TideEventsResponse(year, lat, lon, "Stormglass", events);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Stormglass configuration error.");
            return Problem(ex.Message);
        }
    }
}
