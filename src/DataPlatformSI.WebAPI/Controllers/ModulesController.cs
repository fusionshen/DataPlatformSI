using System;
using System.Linq;
using DataPlatformSI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace DataPlatformSI.WebAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [EnableCors("CorsPolicy")]
    public class ModulesController : Controller
    {
        private readonly string _moduleDirectory = $"{System.AppContext.BaseDirectory}/Downloads/Modules";

        ModuleMetadata[] modules = new ModuleMetadata[]
        {
            new ModuleMetadata { Id = 1, ModuleName = "BasicModule",Checksum ="82240e32a858fbfe5e77b0c920b68e2c", ModuleType="DataPlatformRI.Modules.Basic.BasicModule, DataPlatformRI.Modules.Basic, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" },
            new ModuleMetadata { Id = 2, ModuleName = "UsersModule",Checksum ="82240e32a858fbfe5e77b0c920b6472c", ModuleType="DataPlatformRI.Modules.Users.UsersModule, DataPlatformRI.Modules.Users, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" },
            new ModuleMetadata { Id = 3, ModuleName = "RolesModule",Checksum ="82240e32a858fbfe5e77b0c920b6472c", ModuleType="DataPlatformRI.Modules.Roles.RolesModule, DataPlatformRI.Modules.Roles, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" }
        };

        // GET: api/Modules
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(modules);
        }

        // GET: api/Modules/5
        [HttpGet("{id}")]
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
        public void Post([FromBody] ModuleMetadata value)
        {
        }

        // PUT: api/Modules/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] ModuleMetadata value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        /// <summary>
        /// 下载dll
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //Get: api/Modules/1/Content
        [HttpGet("{id}/Content")]
        public IActionResult GetContentById(int id)
        {
            if (modules.FirstOrDefault(m => m.Id == id) == null)
            {
                return NotFound();
            }
            var addrUrl = $"{_moduleDirectory}/{GetFileNameFromModuleType(modules.FirstOrDefault(m => m.Id == id).ModuleType)}";
            if (!System.IO.File.Exists(addrUrl))
            {
                return NotFound();
            }
            var stream = System.IO.File.OpenRead(addrUrl);
            //return File(stream, "application/vnd.android.package-archive", Path.GetFileName(addrUrl));
            return Ok(stream);
        }

        private string GetFileNameFromModuleType(string moduleType) => $"{moduleType.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)[1]}.dll";

    }
}
 