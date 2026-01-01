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
}
