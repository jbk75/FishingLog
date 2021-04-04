using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FishingLogApi.DAL.Repositories;

namespace FishingLogApi.Controllers
{
    [Produces("application/json")]
    [Route("api/VeidiferdID")]
    public class VeidiferdIDController : Controller
    {
        [HttpGet]
        public string Get()
        { 
            DAL.Logger.Logg("Getting veidiferd nextId");
            DAL.Repositories.VeidiferdirRepository veidiferdirRepo = new VeidiferdirRepository();
            var nextId = veidiferdirRepo.NextVeidiferdId();
            return nextId.ToString();
        }
    }
}