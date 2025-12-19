using FishingLogApi.DAL;
using FishingLogApi.DAL.Models;
using FishingLogApi.DAL.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FishingLogApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FishingPlaceWishlistController : ControllerBase
{
    private readonly FishingPlaceWishlistRepository _wishlistRepository;
    private readonly ILogger<FishingPlaceWishlistController> _logger;

    public FishingPlaceWishlistController(
        FishingPlaceWishlistRepository wishlistRepository,
        ILogger<FishingPlaceWishlistController> logger)
    {
        _wishlistRepository = wishlistRepository;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<IEnumerable<FishingPlaceWishlistItem>> Get()
    {
        try
        {
            var items = _wishlistRepository.GetWishlist();
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching fishing place wishlist");
            return StatusCode(500, "Unable to retrieve wishlist.");
        }
    }

    [HttpPost]
    public ActionResult<FishingPlaceWishlistItem> Post([FromBody] FishingPlaceWishlistCreateRequest request)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        if (request.FishingPlaceId.GetValueOrDefault() <= 0 && string.IsNullOrWhiteSpace(request.FishingPlaceName))
        {
            return BadRequest("Select a fishing place or provide a new fishing place name.");
        }

        if (request.FishingPlaceTypeId == 0)
        {
            return BadRequest("Fishing place type is required.");
        }

        try
        {
            int fishingPlaceId = request.FishingPlaceId.GetValueOrDefault();

            // Create new fishing place if needed
            if (fishingPlaceId == 0 && !string.IsNullOrWhiteSpace(request.FishingPlaceName))
            {
                FishingPlace? existing = VeidistadurRepository.GetByName(request.FishingPlaceName);
                if (existing != null)
                {
                    fishingPlaceId = existing.Id;
                }
                else
                {
                    FishingPlace newPlace = new()
                    {
                        Name = request.FishingPlaceName!,
                        FishingPlaceTypeID = request.FishingPlaceTypeId,
                        Description = request.Description ?? string.Empty
                    };

                    fishingPlaceId = VeidistadurRepository.AddVeidistadur(newPlace);
                    if (fishingPlaceId <= 0)
                    {
                        return StatusCode(500, "Failed to create fishing place.");
                    }
                }
            }

            FishingPlaceWishlistItem item = new()
            {
                FishingPlaceId = fishingPlaceId,
                Description = request.Description ?? string.Empty,
                FishingPlaceTypeId = request.FishingPlaceTypeId
            };

            int newId = _wishlistRepository.AddWishlistItem(item);
            if (newId <= 0)
            {
                return StatusCode(500, "Failed to save wishlist item.");
            }

            FishingPlace? fishingPlace = VeidistadurRepository.GetById(fishingPlaceId);

            item.Id = newId;
            item.FishingPlaceName = fishingPlace?.Name ?? request.FishingPlaceName ?? string.Empty;
            item.FishingPlaceTypeId = fishingPlace?.FishingPlaceTypeID ?? item.FishingPlaceTypeId;
            item.FishingPlaceTypeName = item.FishingPlaceTypeId == 2 ? "River" : item.FishingPlaceTypeId == 1 ? "Lake" : "Unknown";

            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving fishing place wishlist item.");
            return StatusCode(500, "An error occurred while saving the wishlist item.");
        }
    }
}
