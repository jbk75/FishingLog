using Microsoft.AspNetCore.Mvc;
using FishingLogApi.DAL;
using FishingLogApi.DAL.Repositories;

namespace FishingLogApi.Controllers;

[Produces("application/json")]
[Route("api/Veidistadur")]
public class VeidistadurController : Controller
{

    private readonly VeidistadurRepository _repository;

    public VeidistadurController(VeidistadurRepository repository)
    {
        _repository = repository;
    }

    // GET api/veidistadur
    [Route("")]
    [HttpGet]
    public IEnumerable<Veidistadur> Get()
    {
        Logger.Logg("Veidiferdir Get");
        DAL.Repositories.VeidistadurRepository veidistadurRepo = new DAL.Repositories.VeidistadurRepository();
        var result = _repository.GetVeidistadir();
        Logger.Logg("Veidiferdir Get - Done");
        return result;
        //return new string[] { "value1", "value2" };

    }
}