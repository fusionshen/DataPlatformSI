using System;
using System.Linq;
using System.Threading.Tasks;
using DataPlatformSI.Common.GuardToolkit;
using DataPlatformSI.Common.IdentityToolkit;
using DataPlatformSI.Entities.Modules;
using DataPlatformSI.Services.Contracts;
using DataPlatformSI.ViewModels;
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

        Module[] modules = new Module[]
        {
            new Module { Id = 1, Name = "BasicModule",Checksum ="82240e32a858fbfe5e77b0c920b68e2c", ModuleType="DataPlatformRI.Modules.Basic.BasicModule, DataPlatformRI.Modules.Basic, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" },
            new Module { Id = 2, Name = "UsersModule",Checksum ="82240e32a858fbfe5e77b0c920b6472c", ModuleType="DataPlatformRI.Modules.Users.UsersModule, DataPlatformRI.Modules.Users, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" },
            new Module { Id = 3, Name = "RolesModule",Checksum ="82240e32a858fbfe5e77b0c920b6472c", ModuleType="DataPlatformRI.Modules.Roles.RolesModule, DataPlatformRI.Modules.Roles, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" },
            new Module { Id = 4, Name = "MetasModule",Checksum ="82240e32a858fbfe5e77b0c920b6472c", ModuleType="DataPlatformRI.Modules.Metas.MetasModule, DataPlatformRI.Modules.Metas, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" }
        };

        private readonly IModuleService _moduleService;

        public ModulesController(IModuleService moduleService)
        {
            _moduleService = moduleService;
            _moduleService.CheckArgumentIsNull(nameof(_moduleService));
        }

        /// <summary>
        /// 获取模块列表
        /// </summary>
        /// <returns></returns>
        // GET: api/Modules
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Json(await _moduleService.GetAllModulesAsync());
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

        /// <summary>
        /// 新增模块
        /// </summary>
        /// <param name="model">新增所需</param>
        /// <returns></returns>
        // POST: api/Modules
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ModuleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.DumpErrors(false));
            }
            var module = new Module() {
            };
            var result = await _moduleService.AddModuleAsync(module);
            if (!result.Succeeded)
            {
                return BadRequest(result.DumpErrors(useHtmlNewLine: false));
            }
        
            return CreatedAtAction(nameof(Get), new { id = module.Id }, module);
        }

        // PUT: api/Modules/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] Module value)
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
 