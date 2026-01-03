using FishingLogApi.DAL.Models;
using FishingLogApi.DAL.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FishingLogApi.Controllers;

[Produces("application/json")]
[Route("api/fishingnews")]
public sealed class FishingNewsController : ControllerBase
{
    private readonly FishingNewsRepository _repository;

    public FishingNewsController(FishingNewsRepository repository)
    {
        _repository = repository;
    }

    [Route("")]
    [HttpGet]
    public IEnumerable<FishingNewsDto> GetAllFishingNews()
    {
        var list = _repository.GetFishingNews();
        return list;
    }

    [Route("")]
    [HttpPost]
    public ActionResult<FishingNewsDto> CreateFishingNews([FromBody] FishingNewsDto request)
    {
        if (request == null || request.FishingPlaceId <= 0)
        {
            return BadRequest("Fishing place is required.");
        }

        if (request.Date == default)
        {
            return BadRequest("Date is required.");
        }

        var newId = _repository.AddFishingNews(request);
        if (newId <= 0)
        {
            return StatusCode(500, "Unable to save fishing news.");
        }

        request.Id = newId;
        return Ok(request);
    }

    [Route("{id:int}")]
    [HttpDelete]
    public IActionResult DeleteFishingNews(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Invalid fishing news id.");
        }

        var removed = _repository.DeleteFishingNews(id);
        if (!removed)
        {
            return NotFound();
        }

        return NoContent();
    }
}
