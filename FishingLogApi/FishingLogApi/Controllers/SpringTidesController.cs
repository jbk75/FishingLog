using FishingLogApi.Models;
using FishingLogApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FishingLogApi.Controllers;

[ApiController]
[Route("api/spring-tides")]
[Produces("application/json")]
public sealed class SpringTidesController : ControllerBase
{
    private static readonly IReadOnlyDictionary<string, GeoLocation> Locations =
        new Dictionary<string, GeoLocation>(StringComparer.OrdinalIgnoreCase)
        {
            ["reykjavik"] = KnownLocations.Reykjavik,
            ["reykjavík"] = KnownLocations.Reykjavik
        };

    // GET: api/spring-tides/{year}/{month}?location=Reykjavik&windowDays=1
    [HttpGet("{year:int}/{month:int}")]
    public ActionResult<IReadOnlyList<SpringTideDayDto>> Get(
        int year,
        int month,
        [FromQuery] string? location,
        [FromQuery] int? windowDays)
    {
        if (year < 1 || year > 9999)
        {
            return BadRequest("Year must be between 1 and 9999.");
        }

        if (month is < 1 or > 12)
        {
            return BadRequest("Month must be between 1 and 12.");
        }

        var resolvedLocation = ResolveLocation(location);
        if (resolvedLocation is null)
        {
            return BadRequest("Location must be one of: Reykjavik.");
        }

        var resolvedWindowDays = windowDays ?? SpringTideService.DefaultWindowDays;
        if (resolvedWindowDays < 0 || resolvedWindowDays > 5)
        {
            return BadRequest("windowDays must be between 0 and 5.");
        }

        var result = SpringTideService.GetSpringTideDaysForMonth(
            resolvedLocation,
            year,
            month,
            resolvedWindowDays);

        return Ok(result);
    }

    private static GeoLocation? ResolveLocation(string? location)
    {
        if (string.IsNullOrWhiteSpace(location))
        {
            return KnownLocations.Reykjavik;
        }

        return Locations.TryGetValue(location.Trim(), out var resolved)
            ? resolved
            : null;
    }
}
