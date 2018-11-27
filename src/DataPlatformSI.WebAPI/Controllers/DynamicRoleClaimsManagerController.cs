using DataPlatformSI.Common.GuardToolkit;
using DataPlatformSI.Common.IdentityToolkit;
using DataPlatformSI.Services.Contracts.Identity;
using DataPlatformSI.Services.Identity;
using DataPlatformSI.ViewModels.Identity;
using DNTCommon.Web.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataPlatformSI.WebAPI.Controllers
{
    [Authorize(Roles = ConstantRoles.Admin)]
    [Route("api/[controller]")]
    public class DynamicRoleClaimsManagerController : Controller
    {
        private readonly IMvcActionsDiscoveryService _mvcActionsDiscoveryService;
        private readonly IApplicationRoleManager _roleManager;

        public DynamicRoleClaimsManagerController(
            IMvcActionsDiscoveryService mvcActionsDiscoveryService,
            IApplicationRoleManager roleManager)
        {
            _mvcActionsDiscoveryService = mvcActionsDiscoveryService;
            _mvcActionsDiscoveryService.CheckArgumentIsNull(nameof(_mvcActionsDiscoveryService));

            _roleManager = roleManager;
            _roleManager.CheckArgumentIsNull(nameof(_roleManager));
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetRolePermissionList(int? id)
        {
            if (!id.HasValue)
            {
                return BadRequest("no role");
            }

            var role = await _roleManager.FindRoleIncludeRoleClaimsAsync(id.Value);
            if (role == null)
            {
                return NotFound();
            }

            var securedControllerActions = _mvcActionsDiscoveryService.GetAllSecuredControllerActionsWithPolicy(ConstantPolicies.DynamicPermission);
            return Json(new DynamicRoleClaimsManagerViewModel
            {
                SecuredControllerActions = securedControllerActions,
                RoleIncludeRoleClaims = role
            });
        }

        [HttpGet("[action]")]
        public IActionResult GetPermissionList()
        {
            var securedControllerActions = _mvcActionsDiscoveryService.GetAllSecuredControllerActionsWithPolicy(ConstantPolicies.DynamicPermission);
            return Json(securedControllerActions);
        }

        [HttpPost("[action]")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> AddOrUpdateRoleClaims(DynamicRoleClaimsManagerViewModel model)
        {
            var result = await _roleManager.AddOrUpdateRoleClaimsAsync(
                roleId: model.RoleId,
                roleClaimType: ConstantPolicies.DynamicPermissionClaimType,
                selectedRoleClaimValues: model.ActionIds);
            if (!result.Succeeded)
            {
                return BadRequest(error: result.DumpErrors(useHtmlNewLine: true));
            }
            return Json(new { success = true });
        }
    }
}