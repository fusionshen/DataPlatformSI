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
    [DisplayName("主数据-资源仓库模块")]
    [Route("api/[controller]")]
    [EnableCors("CorsPolicy")]
    public class RepositoriesController : Controller
    {
        private readonly IRepositoryService _repoService;
        public RepositoriesController(
            IRepositoryService repoService)
        {
            _repoService = repoService;
            _repoService.CheckArgumentIsNull(nameof(_repoService));
        }

        /// <summary>
        /// 获取主数据资源仓库列表
        /// </summary>
        /// <returns></returns>
        // GET: api/Repositories
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Json(await _repoService.GetAllRepositoriesAsync());
        }

        /// <summary>
        /// 获得单个主数据资源仓库
        /// </summary>
        /// <param name="id">仓库id</param>
        /// <returns></returns>
        // GET: api/Repositories/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            //var module = modules.FirstOrDefault((m) => m.Id == id);
            var repo = await _repoService.GetRepositoryByIdAsync(id);
            if (repo == null)
            {
                return NotFound();
            }
            return Ok(repo);
        }

        /// <summary>
        /// 新增资源仓库
        /// </summary>
        /// <param name="model">新增所需</param>
        /// <returns></returns>
        // POST: api/Repositories
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] RepositoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.DumpErrors(false));
            }
            var repo = new Repository()
            {
                Name = model.Name,
                DisplayName = "金算盘公司主数据资源库",
                Server = model.ServerName,
                UserName = model.UserName,
                Password = model.Password,
                Port = model.Port
            };
            var result = await _repoService.AddRepositoryAsync(repo);
            if (!result.Succeeded)
            {
                return BadRequest(result.DumpErrors(useHtmlNewLine: false));
            }

            return CreatedAtAction(nameof(Get), new { id = repo.Id }, repo);
        }

        /// <summary>
        /// 修改资源仓库信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        // PUT: api/Repositories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] RepositoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.DumpErrors(false));
            }
            var repo = await _repoService.GetRepositoryByIdAsync(id);
            if (repo == null)
            {
                return NotFound();
            }
            repo.Name = model.Name;
            repo.Server = model.ServerName;
            repo.UserName = model.UserName;
            repo.Password = model.Password;
            repo.Port = model.Port;
            var result = await _repoService.UpdateRepositoryAsync(repo);
            if (!result.Succeeded)
            {
                return BadRequest(result.DumpErrors(useHtmlNewLine: false));
            }
            return Json(repo);
        }

        /// <summary>
        /// 删除资源库
        /// </summary>
        /// <param name="id">资源库id</param>
        /// <returns></returns>
        // DELETE: api/Repositories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var repo = await _repoService.GetRepositoryByIdAsync(id);
            if (repo == null)
            {
                return NotFound();
            }
            var result = await _repoService.DeleteRepositoryAsync(repo);
            if (!result.Succeeded)
            {
                return BadRequest(result.DumpErrors(useHtmlNewLine: false));
            }
            return NoContent();
        }
    }
}