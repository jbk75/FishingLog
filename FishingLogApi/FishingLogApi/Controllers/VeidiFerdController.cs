using Microsoft.AspNetCore.Mvc;
using FishingLogApi.DAL.Models;
using FishingLogApi.DAL.Repositories;

namespace FishingLogApi.Controllers;


[Produces("application/json")]
[Route("api/veidiferd")]
public class VeidiFerdController : ControllerBase
{
    private readonly TripRepository _repository;

    public VeidiFerdController(TripRepository repository)
    {
        _repository = repository;
    }

    // GET api/values
    [Route("")]
    [HttpGet]
    public IEnumerable<TripDto> GetAllVeidiferd()
    {
        //DAL.Logger.Logg("Getting veidiferdir");
        var listVeidiferdir = _repository.GetTrips();
        return listVeidiferdir;
        //DAL.Logger.Logg("Veidiferdir Get");
        //return new string[] { "value1", "value2" };
    }

    [Route("{id}")]
    [HttpGet]
    public TripDto GetVeidiferd(int id)
    {
        DAL.Logger.Logg("Getting veidiferdir");
        var listVeidiferdir = _repository.GetTrip(id);//.GetVeidiferdir();
        return listVeidiferdir;
        //DAL.Logger.Logg("Veidiferdir Get");
        //return new string[] { "value1", "value2" };
    }

    /// <summary>  
    /// Delete employee from list.  
    /// </summary>  
    /// <param name="Uid"></param>  
    /// <returns></returns>  
    [Route("{id}")]
    [HttpDelete]
    public HttpResponseMessage DeleteVeidiferd(int id)
    {
        TripDto veidiferd = _repository.GetTrip(id);

        var response = new HttpResponseMessage();
        response.Headers.Add("DeleteMessage", "Succsessfuly Deleted!!!");

        return response;
    }

    // POST api/values
    [HttpPost]
    public ActionResult PostTrip([FromBody]TripDto veidiferd)
    {
        try
        {
            DAL.Logger.Logg("Veidiferdir Post");
            DAL.Logger.Logg("Texti er: " + veidiferd.Description);
            DAL.Logger.Logg("Dagsetning from er: " + veidiferd.DagsFra);
            DAL.Logger.Logg("Dagsetning to er: " + veidiferd.DagsTil);

            DAL.Logger.Logg("Adding veidiferd...");
            _repository.AddTrip(veidiferd);

            DAL.Logger.Logg("Adding veidiferd - DONE!");
            DAL.Logger.Logg("Veidiferdir Post - Done");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        return Ok();
        
    }

    
    /// <summary>
    /// Checks if a fishing trip exists for a given fishing place and date range.
    /// </summary>
    /// <param name="fishingplaceName">The name of the fishing place.</param>
    /// <param name="dateFrom">The start date of the trip (ISO format: yyyy-MM-dd or yyyy-MM-ddTHH:mm:ss).</param>
    /// <param name="dateTo">The end date of the trip (ISO format: yyyy-MM-dd or yyyy-MM-ddTHH:mm:ss).</param>
    /// <returns>True if a trip exists for the specified criteria, otherwise returns false.</returns>
    [Route("exists/{fishingplaceName}/{dateFrom}/{dateTo}")]
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

            bool exists = _repository.TripExists(fishingplaceName, fromDate, toDate);

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
            List<TripDto> trips = _repository.SearchTrips(searchText);

            return Ok(trips);
        }
        catch (Exception ex)
        {
            DAL.Logger.Logg("Error in GetVeidiferdExists: " + ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }


}