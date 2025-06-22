using Microsoft.AspNetCore.Mvc;
using FishingLogApi.DAL.Models;
using FishingLogApi.DAL.Repositories;

namespace FishingLogApi.Controllers
{

    [Produces("application/json")]
    [Route("api/veidiferd")]
    public class VeidiFerdController : Controller
    {

        // GET api/values
        //[ActionName("nextid")]
        //[HttpGet]
        //public string GetNextId()
        //{
        //    DAL.Logger.Logg("Getting veidiferd nextId");
        //    DAL.Repositories.VeidiferdirRepository veidiferdirRepo = new VeidiferdirRepository();
        //    var nextId = veidiferdirRepo.NextVeidiferdId();
        //    return nextId.ToString();
        //    //DAL.Logger.Logg("Veidiferdir Get");
        //    //return new string[] { "value1", "value2" };

        //}

        // GET api/values
        [Route("")]
        [HttpGet]
        public IEnumerable<Veidiferd> GetAllVeidiferd()
        {
            //DAL.Logger.Logg("Getting veidiferdir");
            DAL.Repositories.VeidiferdirRepository veidiferdirRepo = new VeidiferdirRepository();
            var listVeidiferdir = veidiferdirRepo.GetVeidiferdir();
            return listVeidiferdir;
            //DAL.Logger.Logg("Veidiferdir Get");
            //return new string[] { "value1", "value2" };
        }

        //public IHttpActionResult GetProduct(int id)
        //{
        //    var product = products.FirstOrDefault((p) => p.Id == id);
        //    if (product == null)
        //    {
        //        return NotFound();
        //    }
        //    return Ok(product);
        //}
        [Route("{id}")]
        [HttpGet]
        public Veidiferd GetVeidiferd(string id)
        {
            DAL.Logger.Logg("Getting veidiferdir");
            DAL.Repositories.VeidiferdirRepository veidiferdirRepo = new VeidiferdirRepository();
            var listVeidiferdir = veidiferdirRepo.GetVeidiferd(id);//.GetVeidiferdir();
            return listVeidiferdir;
            //DAL.Logger.Logg("Veidiferdir Get");
            //return new string[] { "value1", "value2" };
        }

        //[HttpPost]
        //public void Post(string veidiferd)
        //{
        //    //DAL.Repositories.
        //    DAL.Repositories.VeidiferdirRepository veidiferdRepo = new DAL.Repositories.VeidiferdirRepository();
        //    Veidiferd veidif = new Veidiferd();
        //    veidif.Lysing = "wsfewf";
        //    veidiferdRepo.AddVeidiferd(veidif);
        //}


        /// <summary>  
        /// Delete employee from list.  
        /// </summary>  
        /// <param name="Uid"></param>  
        /// <returns></returns>  

        public HttpResponseMessage DeleteVeidiferd(string id)
        {
            DAL.Repositories.VeidiferdirRepository veidiferdirRepo = new VeidiferdirRepository();
            Veidiferd veidiferd = veidiferdirRepo.GetVeidiferd(id);

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

            //vantar þessa parameters.
            //"VetId": null,
            //"KoId": null,
            //"vsId": null

            //DAL.Logger.Logg("Timastimpill er: " + veidiferd.Timastimpill);
            //DAL.Repositories.
            //Veidiferd ve = new Veidiferd();
            //ve.Lysing = veidiferd.Lysing;
            //ve.DagsFra = veidiferd.DagsFra;
            //ve.DagsTil = veidiferd.DagsTil;
            //ve.Ar = veidiferd.DagsTil.Year.ToString();
            //ve.vsId = veidiferd.VsId;
            DAL.Repositories.VeidiferdirRepository veidiferdRepo = new DAL.Repositories.VeidiferdirRepository();
            DAL.Logger.Logg("Adding veidiferd...");
            veidiferdRepo.AddVeidiferd(veidiferd);
            DAL.Logger.Logg("Adding veidiferd - DONE!");
            DAL.Logger.Logg("Veidiferdir Post - Done");
        }
    }
}