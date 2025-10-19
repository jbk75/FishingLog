using Microsoft.AspNetCore.Mvc;
using FishingLogApi.DAL.Repositories;

namespace FishingLogApi.Controllers;

[Produces("application/json")]
[Route("api/VeidiferdID")]
public class VeidiferdIDController : ControllerBase
{

    private readonly TripRepository _repository;

    public VeidiferdIDController(TripRepository repository)
    {
        _repository = repository;
    }


    [HttpGet]
    public string Get()
    { 
        DAL.Logger.Logg("Getting veidiferd nextId");

        string nextId = _repository.NextTripId();
        return nextId;
    }
}