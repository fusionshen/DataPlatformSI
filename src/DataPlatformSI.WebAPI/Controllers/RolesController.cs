using DataPlatformSI.Services.Contracts.Identity;
using DataPlatformSI.Common.IdentityToolkit;
using DataPlatformSI.Common.GuardToolkit;
using DataPlatformSI.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataPlatformSI.ViewModels.Identity;
using System.Data.SqlClient;
using DataPlatformSI.Entities.Identity;

namespace DataPlatformSI.WebAPI.Controllers
{
    /// <summary>
    /// 角色管理
    /// </summary>
    [Authorize(Roles = ConstantRoles.Admin)]
    [Route("api/[controller]")]
    [EnableCors("CorsPolicy")]
    public class RolesController : Controller
    {
        private const string RoleNotFound = "角色不存在";
        private const int DefaultPageSize = 7;

        private readonly IApplicationRoleManager _roleManager;

        public RolesController(IApplicationRoleManager roleManager)
        {
            _roleManager = roleManager;
            _roleManager.CheckArgumentIsNull(nameof(_roleManager));
        }

        /// <summary>
        /// 获得角色列表
        /// </summary>
        /// <returns>期望返回</returns>
        [HttpGet("[action]")]
        public async Task<IActionResult> GetRoleList()
        {
            return Json(await _roleManager.GetAllCustomRolesAndUsersCountListAsync());
        }

        /// <summary>
        /// restful获得角色列表
        /// </summary>
        /// <returns>期望返回</returns>
        /// GET: api/Modules
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Json(await _roleManager.GetAllCustomRolesAndUsersCountListAsync());
        }


        /// <summary>
        /// 编辑角色
        /// </summary>
        /// <param name="model">角色实体</param>
        /// <returns>期望返回</returns>
        [HttpPost("[action]")]
        public async Task<IActionResult> EditRole(RoleViewModel model)
        {
     
            var role = await _roleManager.FindByIdAsync(model.Id);
            if (role == null)
            {
                return BadRequest(RoleNotFound);
            }
            role.Name = model.Name;
            var result = await _roleManager.UpdateAsync(role);
            return Json(result.Succeeded ? "true" : result.DumpErrors(useHtmlNewLine: true));
        }

        /// <summary>
        /// restful编辑角色
        /// </summary>
        /// <param name="id">角色id</param>
        /// <param name="model">实体</param>
        /// <returns>期望返回</returns>
        // PUT: api/Roles/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] RoleViewModel model)
        {

            var role = await _roleManager.FindByIdAsync(model.Id);
            if (role == null)
            {
                return Json(RoleNotFound);
            }
            role.Name = model.Name;
            var result = await _roleManager.UpdateAsync(role);
            return Json(result.Succeeded ? "true" : result.DumpErrors(useHtmlNewLine: true));
        }

        /// <summary>
        /// 新增角色
        /// </summary>
        /// <param name="model">新增角色实体</param>
        /// <returns>期望返回</returns>
        [HttpPost("[action]")]
        public async Task<IActionResult> AddRole(RoleViewModel model)
        {
            var result = await _roleManager.CreateAsync(new Role(model.Name));
            return Json(result.Succeeded ? "true" : result.DumpErrors(useHtmlNewLine: true));
        }

        /// <summary>
        /// restful新增角色
        /// </summary>
        /// <param name="model"></param>
        /// <returns>期望返回</returns>
        // POST: api/Roles
        [HttpPost]
        public async Task<IActionResult> Post(RoleViewModel model)
        {
            var result = await _roleManager.CreateAsync(new Role(model.Name));
            return Json(result.Succeeded ? "true" : result.DumpErrors(useHtmlNewLine: true));
        }

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="model">所需实体</param>
        /// <returns>期望返回</returns>
        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteRole(RoleViewModel model)
        {
            var role = await _roleManager.FindByIdAsync(model.Id);
            if (role == null)
            {
                return Json(RoleNotFound);
            }
            var result = await _roleManager.DeleteAsync(role);
            return Json(result.Succeeded ? "true" : result.DumpErrors(useHtmlNewLine: true));

        }

        /// <summary>
        /// restful删除角色
        /// </summary>
        /// <param name="id">角色id</param>
        /// <returns></returns>
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
            {
                return Json(RoleNotFound);
            }
            var result = await _roleManager.DeleteAsync(role);
            return Json(result.Succeeded ? "true" : result.DumpErrors(useHtmlNewLine: true));
        }

        /// <summary>
        /// 角色下用户列表
        /// </summary>
        /// <param name="id">角色id</param>
        /// <param name="page">第几页</param>
        /// <param name="field">排序列</param>
        /// <param name="order">排序方式</param>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<IActionResult> UsersInRole(int? id, int? page = 1, string field = "Id", SortOrder order = SortOrder.Ascending)
        {
            if (id == null)
            {
                return BadRequest("Error");
            }

            var model = await _roleManager.GetPagedApplicationUsersInRoleListAsync(
                roleId: id.Value,
                pageNumber: page.Value - 1,
                recordsPerPage: DefaultPageSize,
                sortByField: field,
                sortOrder: order,
                showAllUsers: true);

            return Json(model);
        }
    }
}