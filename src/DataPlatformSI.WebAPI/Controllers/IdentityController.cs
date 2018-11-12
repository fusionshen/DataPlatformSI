using DataPlatformSI.Services.Contracts.Identity;
using Microsoft.AspNetCore.Mvc;
using DataPlatformSI.Common.GuardToolkit;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace DataPlatformSI.WebAPI.Controllers
{
    [Route("api/[controller]")]
    public class IdentityController : Controller
    {
        private readonly IActionPermissionService _actionPermissionService;

        public IdentityController(IActionPermissionService actionPermissionService)
        {
            _actionPermissionService = actionPermissionService;
            _actionPermissionService.CheckArgumentIsNull(nameof(actionPermissionService));
        }

        [AllowAnonymous]
        [HttpGet("[action]")]
        public IActionResult GetPermissionList()
        {
            return Ok(_actionPermissionService.GetAllActionByAssembly("DataPlatformSI.WebAPI"));
        }
    }  
}
