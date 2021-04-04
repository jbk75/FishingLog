using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FishingLogApi.DAL;

namespace FishingLogApi.Controllers
{
    [Produces("application/json")]
    [Route("api/Veidistadur")]
    public class VeidistadurController : Controller
    {
        // GET api/veidistadur
        [Route("")]
        [HttpGet]
        public IEnumerable<Veidistadur> Get()
        {
            Logger.Logg("Veidiferdir Get");
            DAL.Repositories.VeidistadurRepository veidistadurRepo = new DAL.Repositories.VeidistadurRepository();
            var result = veidistadurRepo.GetVeidistadir();
            Logger.Logg("Veidiferdir Get - Done");
            return result;
            //return new string[] { "value1", "value2" };

        }
    }
}