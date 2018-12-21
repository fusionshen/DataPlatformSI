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
    [Area("DataPlatformRI.Modules.Test2")]
    [DisplayName("测试2模块")]
    public class DynamicPermissionsSampleController : Controller
    {
        [DisplayName("Index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(RoleViewModel model)
        {
            return View(model);
        }

        [DisplayName("Books")]
        public IActionResult Books()
        {
            return View(viewName: "Index");
        }

        [DisplayName("Users")]
        public IActionResult Users()
        {
            return View(viewName: "Index");
        }

        [DisplayName("BooksGiven")]
        public IActionResult BooksGiven()
        {
            return View(viewName: "Index");
        }

        [DisplayName("BooksMissings")]
        public IActionResult BooksMissings()
        {
            return View(viewName: "Index");
        }
    }
}