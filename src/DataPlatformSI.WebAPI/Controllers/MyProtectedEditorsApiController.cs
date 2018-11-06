using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataPlatformSI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DataPlatformSI.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("CorsPolicy")]
    //[Authorize(Policy = CustomRoles.Editor)]
    public class MyProtectedEditorsApiController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                Id = 1,
                Title = "Hello from My Protected Editors Controller! [Authorize(Policy = CustomRoles.Editor)]",
                Username = this.User.Identity.Name
            });
        }
    }
}