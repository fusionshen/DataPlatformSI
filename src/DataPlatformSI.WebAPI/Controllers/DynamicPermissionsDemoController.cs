using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using DataPlatformSI.Services.Identity;
using DataPlatformSI.ViewModels.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DataPlatformSI.WebAPI.Controllers
{
    [Authorize(Policy = ConstantPolicies.DynamicPermission)]
    [Area("DataPlatformRI.Modules.Test1")]
    [DisplayName("测试1模块")]
    public class DynamicPermissionsDemoController : Controller
    {
        [HttpPost]
        public IActionResult Index([FromBody]RoleViewModel model)
        {
            return Json(model);
        }

        [DisplayName("Products")]
        public IActionResult Products()
        {
            return View(viewName: "Index");
        }

        [DisplayName("Orders")]
        public IActionResult Orders()
        {
            return View(viewName: "Index");
        }

        [DisplayName("Sells")]
        public IActionResult Sells()
        {
            return View(viewName: "Index");
        }

        [DisplayName("Customers")]
        public IActionResult Customers()
        {
            return View(viewName: "Index");
        }
    }
}

