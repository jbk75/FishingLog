using Microsoft.AspNetCore.Mvc;
using FishingLogApi.DAL.Repositories;

namespace FishingLogApi.Controllers;

[Produces("application/json")]
[Route("api/VeidiferdID")]
public class VeidiferdIDController : Controller
{

    private readonly VeidiferdirRepository _repository;

    public VeidiferdIDController(VeidiferdirRepository repository)
    {
        _repository = repository;
    }


    [HttpGet]
    public string Get()
    { 
        DAL.Logger.Logg("Getting veidiferd nextId");
        DAL.Repositories.VeidiferdirRepository veidiferdirRepo = new VeidiferdirRepository();
        var nextId = _repository.NextVeidiferdId();
        return nextId.ToString();
    }
}