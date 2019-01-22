using System.ComponentModel;
using System.Threading.Tasks;
using DataPlatformSI.Services.Contracts.MDM;
using DataPlatformSI.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataPlatformSI.Common.GuardToolkit;
using DataPlatformSI.Common.IdentityToolkit;
using DataPlatformSI.ViewModels.MDM;
using DataPlatformSI.Entities.MDM;
using Microsoft.AspNetCore.Cors;

namespace DataPlatformSI.WebAPI.Controllers
{
    [Authorize(Policy = ConstantPolicies.DynamicPermission)]
    [Area("DataPlatformRI.Modules.MDM")]
    [DisplayName("主数据-服务器模块")]
    [Route("api/[controller]")]
    [EnableCors("CorsPolicy")]
    public class MDMServersController : Controller
    {
        private readonly IMDMServerService _serverService;
        public MDMServersController(
            IMDMServerService serverService)
        {
            _serverService = serverService;
            _serverService.CheckArgumentIsNull(nameof(_serverService));
        }

        /// <summary>
        /// 获取主数据服务器列表
        /// </summary>
        /// <returns></returns>
        // GET: api/MDMServers
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Json(await _serverService.GetAllServersAsync());
        }

        /// <summary>
        /// 获得单个主数据服务器
        /// </summary>
        /// <param name="id">服务器id</param>
        /// <returns></returns>
        // GET: api/MDMServers/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            //var module = modules.FirstOrDefault((m) => m.Id == id);
            var server = await _serverService.GetServerByIdAsync(id);
            if (server == null)
            {
                return NotFound();
            }
            return Ok(server);
        }

        /// <summary>
        /// 新增服务器
        /// </summary>
        /// <param name="model">新增所需</param>
        /// <returns></returns>
        // POST: api/MDMServers
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]MDMServerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.DumpErrors(false));
            }
            var server = new MDMServer()
            {
                Name = model.Name
            };
            var result = await _serverService.AddServerAsync(server);
            if (!result.Succeeded)
            {
                return BadRequest(result.DumpErrors(useHtmlNewLine: false));
            }

            return CreatedAtAction(nameof(Get), new { id = server.Id }, server);
        }

        /// <summary>
        /// 修改主数据服务器信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        // PUT: api/MDMServers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] MDMServerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.DumpErrors(false));
            }
            var server = await _serverService.GetServerByIdAsync(id);
            if (server == null)
            {
                return NotFound();
            }
            server.Name = model.Name;
            var result = await _serverService.UpdateServerAsync(server);
            if (!result.Succeeded)
            {
                return BadRequest(result.DumpErrors(useHtmlNewLine: false));
            }
            return Json(server);
        }

        /// <summary>
        /// 删除主数据服务器
        /// </summary>
        /// <param name="id">服务器id</param>
        /// <returns></returns>
        // DELETE: api/MDMServers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var server = await _serverService.GetServerByIdAsync(id);
            if (server == null)
            {
                return NotFound();
            }
            var result = await _serverService.DeleteServerAsync(server);
            if (!result.Succeeded)
            {
                return BadRequest(result.DumpErrors(useHtmlNewLine: false));
            }
            return NoContent();
        }
    }
}
