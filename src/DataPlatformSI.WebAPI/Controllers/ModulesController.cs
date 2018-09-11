using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataPlatformSI.DomainClasses;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DataPlatformSI.WebAPI.Controllers
{
    [Route("api/[controller]")]
    //[EnableCors("CorsPolicy")]
    public class ModulesController : Controller
    {
        ModuleMetadata[] modules = new ModuleMetadata[]
        {
            new ModuleMetadata { Id = 1, ModuleName = "BasicModule",Checksum ="82240e32a858fbfe5e77b0c920b68e2c", ModuleType="DataPlatformRI.Modules.Basic.BasicModule, DataPlatformRI.Modules.Basic, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" }
        };

        // GET: api/Modules
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(modules);
        }

        // GET: api/Modules/5
        [HttpGet("{id}", Name = "Get")]
        public IActionResult Get(int id)
        {
            var module = modules.FirstOrDefault((m) => m.Id == id);
            if (module == null)
            {
                return NotFound();
            }
            return Ok(module);
        }

        // POST: api/Modules
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Modules/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
