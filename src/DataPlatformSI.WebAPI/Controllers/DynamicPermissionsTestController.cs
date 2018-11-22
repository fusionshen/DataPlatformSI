using DataPlatformSI.Services.Identity;
using DataPlatformSI.ViewModels.Identity;
using DNTBreadCrumb.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace DataPlatformSI.WebAPI.Controllers
{
    [Authorize(Policy = ConstantPolicies.DynamicPermission)]
    [DisplayName("DynamicPermissionsTestController")]
    public class DynamicPermissionsTestController : Controller
    {
        [DisplayName("Index")]
        [BreadCrumb(Order = 1)]
        public IActionResult Index()
        {
            return View();
        }

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
