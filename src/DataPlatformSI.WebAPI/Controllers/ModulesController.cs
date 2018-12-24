using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataPlatformSI.Common.GuardToolkit;
using DataPlatformSI.Common.IdentityToolkit;
using DataPlatformSI.Entities.Modules;
using DataPlatformSI.Services.Contracts;
using DataPlatformSI.Services.Identity;
using DataPlatformSI.ViewModels;
using DNTCommon.Web.Core;
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

        readonly Module[] modules = new Module[]
        {
            new Module { Id = 1, Name = "BasicModule",Checksum ="82240e32a858fbfe5e77b0c920b68e2c", SpaceName ="DataPlatformRI.Modules.Basic",Version ="1.0.0.0" },
            new Module { Id = 2, Name = "UsersModule",Checksum ="82240e32a858fbfe5e77b0c920b6472c", SpaceName ="DataPlatformRI.Modules.Users",Version ="1.0.0.0" },
            new Module { Id = 3, Name = "RolesModule",Checksum ="82240e32a858fbfe5e77b0c920b6472c", SpaceName ="DataPlatformRI.Modules.Roles",Version ="1.0.0.0" },
            new Module { Id = 4, Name = "MetasModule",Checksum ="82240e32a858fbfe5e77b0c920b6472c", SpaceName ="DataPlatformRI.Modules.Metas",Version ="1.0.0.0" }
        };

        private readonly IModuleService _moduleService;

        private readonly IMvcActionsDiscoveryService _mvcActionsDiscoveryService;

        public ModulesController(
            IModuleService moduleService,
            IMvcActionsDiscoveryService mvcActionsDiscoveryService
            )
        {
            _moduleService = moduleService;
            _moduleService.CheckArgumentIsNull(nameof(_moduleService));

            _mvcActionsDiscoveryService = mvcActionsDiscoveryService;
            _mvcActionsDiscoveryService.CheckArgumentIsNull(nameof(_mvcActionsDiscoveryService));
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

        /// <summary>
        /// 获得单个模块
        /// </summary>
        /// <param name="id">模块id</param>
        /// <returns></returns>
        // GET: api/Modules/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            //var module = modules.FirstOrDefault((m) => m.Id == id);
            var module = await _moduleService.GetModuleByIdAsync(id);
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
                Name = model.Name,
                SpaceName = model.SpaceName,
                DisplayName = model.DisplayName,
                Checksum = Guid.NewGuid().ToString("N"),
                Description = model.Description,
                CreatedTime = DateTimeOffset.UtcNow,
                Version = model.Version,
            };
            var result = await _moduleService.AddModuleAsync(module);
            if (!result.Succeeded)
            {
                return BadRequest(result.DumpErrors(useHtmlNewLine: false));
            }
        
            return CreatedAtAction(nameof(Get), new { id = module.Id }, module);
        }

        /// <summary>
        /// 修改模块信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        // PUT: api/Modules/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ModuleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.DumpErrors(false));
            }
            var module = await _moduleService.GetModuleByIdAsync(id);
            if (module == null)
            {
                return NotFound();
            }
            if (!module.IsCore.HasValue || !(bool)module.IsCore)
            {
                module.Name = model.Name;
                module.SpaceName = model.SpaceName;
            }
            module.DisplayName = model.DisplayName;
            module.Description = model.Description;
            module.Version = model.Version;
            var result = await _moduleService.UpdateModuleAsync(module);
            if (!result.Succeeded)
            {
                return BadRequest(result.DumpErrors(useHtmlNewLine: false));
            }
            return Json(module);
        }

        /// <summary>
        /// 删除模块
        /// </summary>
        /// <param name="id">模块id</param>
        /// <returns></returns>
        // DELETE: api/Modules/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var module = await _moduleService.GetModuleByIdAsync(id);
            if (module == null)
            {
                return NotFound();
            }
            var result = await _moduleService.DeleteModuleAsync(module);
            if (!result.Succeeded)
            {
                return BadRequest(result.DumpErrors(useHtmlNewLine: false));
            }
            return NoContent();
        }

        /// <summary>
        /// 下载dll
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //Get: api/Modules/1/Content
        [HttpGet("{id}/Content")]
        public async Task<IActionResult> GetContentById(int id)
        {
            var module = await _moduleService.GetModuleByIdAsync(id);
            if (module == null)
            {
                return NotFound();
            }
            //var addrUrl = $"{_moduleDirectory}/{GetFileNameFromModuleType(module.ModuleType)}";
            var addrUrl = $"{_moduleDirectory}/{module.SpaceName}.dll";
            if (!System.IO.File.Exists(addrUrl))
            {
                return NotFound();
            }
            var stream = System.IO.File.OpenRead(addrUrl);
            //return File(stream, "application/vnd.android.package-archive", Path.GetFileName(addrUrl));
            return Ok(stream);
        }

        private string GetFileNameFromModuleType(string moduleType) => $"{moduleType.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)[1]}.dll";

        /// <summary>
        /// 初始化内部模块
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<IActionResult> Init()
        {
            var securedControllerActions = _mvcActionsDiscoveryService.GetAllSecuredControllerActionsWithPolicy(ConstantPolicies.DynamicPermission);
            var expects = securedControllerActions.Where(ca => !string.IsNullOrWhiteSpace(ca.AreaName)).Select(o => new Module() {
                Name = $"{o.AreaName.Split(".").Last()}Module",
                SpaceName = o.AreaName,
                DisplayName = o.ControllerDisplayName
            }).ToList();
            
            var result = await _moduleService.InitModuleAsync(expects);
            if (!result.Succeeded)
            {
                return BadRequest(error: result.DumpErrors(useHtmlNewLine: false));
            }
            return Ok(new { Success = true });
        }

    }
}
 