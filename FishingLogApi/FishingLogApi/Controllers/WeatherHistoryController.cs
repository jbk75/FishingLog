using FishingLogApi.Models;
using FishingLogApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FishingLogApi.Controllers;

[ApiController]
[Route("api/weather-history")]
public sealed class WeatherHistoryController : ControllerBase
{
    private const double DefaultLatitude = 64.1466;
    private const double DefaultLongitude = -21.9426;
    private readonly WeatherHistoryService _weatherHistoryService;

    public WeatherHistoryController(WeatherHistoryService weatherHistoryService)
    {
        _weatherHistoryService = weatherHistoryService;
    }

    [HttpGet]
    public async Task<ActionResult<WeatherHistoryResponse>> Get(
        [FromQuery] int year,
        [FromQuery] double? latitude,
        [FromQuery] double? longitude,
        CancellationToken cancellationToken)
    {
        var currentYear = DateTimeOffset.UtcNow.Year;
        if (year < 1970 || year > currentYear)
        {
            return BadRequest("Year must be between 1970 and the current year.");
        }

        var resolvedLatitude = latitude ?? DefaultLatitude;
        var resolvedLongitude = longitude ?? DefaultLongitude;

        var weatherHistory = await _weatherHistoryService.GetWeatherHistoryAsync(
            year,
            resolvedLatitude,
            resolvedLongitude,
            cancellationToken);

        return Ok(weatherHistory);
    }
}
