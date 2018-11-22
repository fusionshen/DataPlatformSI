using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DataPlatformSI.WebAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [EnableCors("CorsPolicy")]
    public class MyProtectedApiController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                Id = 1,
                Title = "Hello from My Protected Controller! [Authorize]",
                Username = this.User.Identity.Name
            });
        }
    }
}