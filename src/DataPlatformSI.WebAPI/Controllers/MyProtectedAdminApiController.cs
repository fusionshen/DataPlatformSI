using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataPlatformSI.Services;
using DataPlatformSI.Common.GuardToolkit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DataPlatformSI.Services.Contracts.Identity;
using DataPlatformSI.Services.Identity;

namespace DataPlatformSI.WebAPI.Controllers
{
    [Authorize(Roles = ConstantRoles.Admin)]
    [Route("api/[controller]")]
    [EnableCors("CorsPolicy")]
    public class MyProtectedAdminApiController : Controller
    {
        private readonly IApplicationUserManager _userManager;

        public MyProtectedAdminApiController(IApplicationUserManager userManager)
        {
            _userManager = userManager;
            _userManager.CheckArgumentIsNull(nameof(userManager));
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var userDataClaim = claimsIdentity.FindFirst(ClaimTypes.UserData);
            var userId = userDataClaim.Value;

            return Ok(new
            {
                Id = 1,
                Title = "Hello from My Protected Admin Api Controller! [Authorize(Roles = ConstantRoles.Admin)]",
                Username = this.User.Identity.Name,
                UserData = userId,
                TokenSerialNumber = await _userManager.GetSerialNumberAsync(int.Parse(userId)),
                Roles = claimsIdentity.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToList()
            });
        }
    }
}