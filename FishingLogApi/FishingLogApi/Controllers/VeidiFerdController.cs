using Microsoft.AspNetCore.Mvc;
using FishingLogApi.DAL.Models;
using FishingLogApi.DAL.Repositories;

namespace FishingLogApi.Controllers;


[Produces("application/json")]
[Route("api/veidiferd")]
public class VeidiFerdController : Controller
{
    private readonly VeidiferdirRepository _repository;

    public VeidiFerdController(VeidiferdirRepository repository)
    {
        _repository = repository;
    }

    // GET api/values
    [Route("")]
    [HttpGet]
    public IEnumerable<Veidiferd> GetAllVeidiferd()
    {
        //DAL.Logger.Logg("Getting veidiferdir");
        DAL.Repositories.VeidiferdirRepository veidiferdirRepo = new();
        var listVeidiferdir = _repository.GetVeidiferdir();
        return listVeidiferdir;
        //DAL.Logger.Logg("Veidiferdir Get");
        //return new string[] { "value1", "value2" };
    }

    [Route("{id}")]
    [HttpGet]
    public Veidiferd GetVeidiferd(string id)
    {
        DAL.Logger.Logg("Getting veidiferdir");
        DAL.Repositories.VeidiferdirRepository veidiferdirRepo = new VeidiferdirRepository();
        var listVeidiferdir = _repository.GetVeidiferd(id);//.GetVeidiferdir();
        return listVeidiferdir;
        //DAL.Logger.Logg("Veidiferdir Get");
        //return new string[] { "value1", "value2" };
    }

    /// <summary>  
    /// Delete employee from list.  
    /// </summary>  
    /// <param name="Uid"></param>  
    /// <returns></returns>  

    public HttpResponseMessage DeleteVeidiferd(string id)
    {
        DAL.Repositories.VeidiferdirRepository veidiferdirRepo = new VeidiferdirRepository();
        Veidiferd veidiferd = _repository.GetVeidiferd(id);

        //if (veidiferd == null)
        //{
        //    throw new System.Web.HttpResponseException(HttpStatusCode.NotFound);
        //}
        //_emp.Remove(emp);
        var response = new HttpResponseMessage();
        response.Headers.Add("DeleteMessage", "Succsessfuly Deleted!!!");
        return response;
    }

    // POST api/values
    [HttpPost]
    public void Post([FromBody]Veidiferd veidiferd)
    {
        DAL.Logger.Logg("Veidiferdir Post");
        DAL.Logger.Logg("Texti er: " + veidiferd.Lysing);
        DAL.Logger.Logg("Dagsetning from er: " + veidiferd.DagsFra);
        DAL.Logger.Logg("Dagsetning to er: " + veidiferd.DagsTil);

        _ = new
        DAL.Repositories.VeidiferdirRepository();
        
        DAL.Logger.Logg("Adding veidiferd...");
        _repository.AddVeidiferd(veidiferd);

        DAL.Logger.Logg("Adding veidiferd - DONE!");
        DAL.Logger.Logg("Veidiferdir Post - Done");
    }

    [Route("exists/{fishingplaceid}/{dateFrom}/{dateTo}")]
    [HttpGet]
    public ActionResult<bool> GetVeidiferdExists(string fishingplaceName, string dateFrom, string dateTo)
    {
        try
        {
            DAL.Logger.Logg("Checking if Veidiferd exists");

            // Parse incoming date strings
            if (!DateTime.TryParse(dateFrom, out DateTime fromDate) ||
                !DateTime.TryParse(dateTo, out DateTime toDate))
            {
                return BadRequest("Invalid date format. Use ISO format: yyyy-MM-dd or yyyy-MM-ddTHH:mm:ss");
            }

            VeidiferdirRepository veidiferdirRepo = new();
            bool exists = veidiferdirRepo.VeidiferdExists(fishingplaceName, fromDate, toDate);

            return Ok(exists);
        }
        catch (Exception ex)
        {
            DAL.Logger.Logg("Error in GetVeidiferdExists: " + ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }


}