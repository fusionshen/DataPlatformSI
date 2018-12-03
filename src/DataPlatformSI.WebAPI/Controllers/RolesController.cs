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
        public async Task<IActionResult> EditRole([FromBody] RoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.DumpErrors(false));
            }
            var role = await _roleManager.FindByIdAsync(model.Id);
            if (role == null)
            {
                return BadRequest(RoleNotFound);
            }
            if (role.NormalizedName != "ADMIN")
            {
                role.Name = model.Name;
            }
            role.DisplayName = model.DisplayName;
            role.Description = model.Description;
            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                return BadRequest(result.DumpErrors(useHtmlNewLine: false));
            }
            if (role.NormalizedName != "ADMIN")
            {
                result = await _roleManager.AddOrUpdateRoleClaimsAsync(
                   roleId: role.Id,
                   roleClaimType: ConstantPolicies.DynamicPermissionClaimType,
                   selectedRoleClaimValues: model.ActionIds);
                if (!result.Succeeded)
                {
                    return BadRequest(error: result.DumpErrors(useHtmlNewLine: false));
                }
            }
            return Json(await _roleManager.FindRoleIncludeRoleClaimsAsync(role.Id));
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.DumpErrors(false));
            }
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
            {
                return BadRequest(RoleNotFound);
            }
            if (role.NormalizedName != "ADMIN")
            {
                role.Name = model.Name;
            }
            role.DisplayName = model.DisplayName;
            role.Description = model.Description;
            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                return BadRequest(result.DumpErrors(useHtmlNewLine: false));
            }
            if (role.NormalizedName != "ADMIN")
            {
                result = await _roleManager.AddOrUpdateRoleClaimsAsync(
                    roleId: role.Id,
                    roleClaimType: ConstantPolicies.DynamicPermissionClaimType,
                    selectedRoleClaimValues: model.ActionIds);
                if (!result.Succeeded)
                {
                    return BadRequest(error: result.DumpErrors(useHtmlNewLine: false));
                }
            }
            return Json(await _roleManager.FindRoleIncludeRoleClaimsAsync(id));
            //return Json(result.Succeeded ? "true" : result.DumpErrors(useHtmlNewLine: false));
        }

        /// <summary>
        /// 新增角色
        /// </summary>
        /// <param name="model">新增角色实体</param>
        /// <returns>期望返回</returns>
        [HttpPost("[action]")]
        public async Task<IActionResult> AddRole([FromBody] RoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.DumpErrors(false));
            }
            var role = new Role(model.Name, model.DisplayName, model.Description);
            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                return BadRequest(result.DumpErrors(useHtmlNewLine: false));
            }
            if (model.ActionIds.Count() != 0)
            {
                result = await _roleManager.AddOrUpdateRoleClaimsAsync(
                    roleId: role.Id,
                    roleClaimType: ConstantPolicies.DynamicPermissionClaimType,
                    selectedRoleClaimValues: model.ActionIds);
                if (!result.Succeeded)
                {
                    return BadRequest(error: result.DumpErrors(useHtmlNewLine: false));
                }
            }

            return CreatedAtAction(nameof(Get), new { id = role.Id }, role);
        }

        /// <summary>
        /// restful新增角色
        /// </summary>
        /// <param name="model"></param>
        /// <returns>期望返回</returns>
        // POST: api/Roles
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] RoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.DumpErrors(false));
            }
            var role = new Role(model.Name,model.DisplayName,model.Description);
            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                return BadRequest(result.DumpErrors(useHtmlNewLine: false));
            }
            if (model.ActionIds.Count() != 0)
            {
                result = await _roleManager.AddOrUpdateRoleClaimsAsync(
                    roleId: role.Id,
                    roleClaimType: ConstantPolicies.DynamicPermissionClaimType,
                    selectedRoleClaimValues: model.ActionIds);
                if (!result.Succeeded)
                {
                    return BadRequest(error: result.DumpErrors(useHtmlNewLine: false));
                }
            }
            
            return CreatedAtAction(nameof(Get), new { id = role.Id }, role);
            //return Json(result.Succeeded ? "true" : result.DumpErrors(useHtmlNewLine: false));
        }

        /// <summary>
        /// 获取单个角色
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: api/Roles/5
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var role = await _roleManager.FindRoleIncludeRoleClaimsAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return Ok(role);
        }

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="model">所需实体</param>
        /// <returns>期望返回</returns>
        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteRole([FromBody] RoleViewModel model)
        {
            var role = await _roleManager.FindByIdAsync(model.Id);
            if (role == null)
            {
                return BadRequest(RoleNotFound);
            }
            if (role.NormalizedName == "ADMIN")
            {
                return BadRequest("ADMIN ROLE CANT BE REMOVED");
            }
            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                return BadRequest(result.DumpErrors(useHtmlNewLine: false));
            }
            return NoContent();
        }

        /// <summary>
        /// restful删除角色
        /// </summary>
        /// <param name="id">角色id</param>
        /// <returns></returns>
        // DELETE: api/Roles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
            {
                return BadRequest(RoleNotFound);
            }
            if (role.NormalizedName == "ADMIN")
            {
                return BadRequest("ROLE ADMIN CANT BE REMOVED");
            }
            //result = await _roleManager.AddOrUpdateRoleClaimsAsync(
            //        roleId: role.Id,
            //        roleClaimType: ConstantPolicies.DynamicPermissionClaimType,
            //        selectedRoleClaimValues: null);
            //if (!result.Succeeded)
            //{
            //    return BadRequest(error: result.DumpErrors(useHtmlNewLine: false));
            //}
            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                return BadRequest(result.DumpErrors(useHtmlNewLine: false));
            }
            return NoContent();
            //return Json(result.Succeeded ? "true" : result.DumpErrors(useHtmlNewLine: false));
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