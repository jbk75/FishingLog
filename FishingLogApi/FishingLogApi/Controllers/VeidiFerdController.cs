using Microsoft.AspNetCore.Mvc;
using FishingLogApi.DAL.Models;
using FishingLogApi.DAL.Repositories;

namespace FishingLogApi.Controllers;


[Produces("application/json")]
[Route("api/veidiferd")]
public class VeidiFerdController : ControllerBase
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
        var listVeidiferdir = _repository.GetVeidiferdir();
        return listVeidiferdir;
        //DAL.Logger.Logg("Veidiferdir Get");
        //return new string[] { "value1", "value2" };
    }

    [Route("{id}")]
    [HttpGet]
    public Veidiferd GetVeidiferd(int id)
    {
        DAL.Logger.Logg("Getting veidiferdir");
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

    public HttpResponseMessage DeleteVeidiferd(int id)
    {
        Veidiferd veidiferd = _repository.GetVeidiferd(id);

        var response = new HttpResponseMessage();
        response.Headers.Add("DeleteMessage", "Succsessfuly Deleted!!!");

        return response;
    }

    // POST api/values
    [HttpPost]
    public ActionResult PostTrip([FromBody]Veidiferd veidiferd)
    {
        try
        {
            DAL.Logger.Logg("Veidiferdir Post");
            DAL.Logger.Logg("Texti er: " + veidiferd.Description);
            DAL.Logger.Logg("Dagsetning from er: " + veidiferd.DagsFra);
            DAL.Logger.Logg("Dagsetning to er: " + veidiferd.DagsTil);

            DAL.Logger.Logg("Adding veidiferd...");
            _repository.AddVeidiferd(veidiferd);

            DAL.Logger.Logg("Adding veidiferd - DONE!");
            DAL.Logger.Logg("Veidiferdir Post - Done");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        return Ok();
        
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

            bool exists = _repository.VeidiferdExists(fishingplaceName, fromDate, toDate);

            return Ok(exists);
        }
        catch (Exception ex)
        {
            DAL.Logger.Logg("Error in GetVeidiferdExists: " + ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Search for a veidiferd/Trip by text
    /// </summary>
    [Route("search")]
    [HttpGet]
    public ActionResult<bool> GetSearch(string searchText)
    {
        try
        {
            List<Veidiferd> trips = _repository.SearchTrips(searchText);

            return Ok(trips);
        }
        catch (Exception ex)
        {
            DAL.Logger.Logg("Error in GetVeidiferdExists: " + ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }


}