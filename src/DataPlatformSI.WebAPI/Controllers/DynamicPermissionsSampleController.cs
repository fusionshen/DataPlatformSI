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
    [BreadCrumb(UseDefaultRouteUrl = true, Order = 0)]
    [DisplayName("DynamicPermissionsSampleController")]
    public class DynamicPermissionsSampleController : Controller
    {
        [DisplayName("Index")]
        [BreadCrumb(Order = 1)]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Index(RoleViewModel model)
        {
            return View(model);
        }

        [DisplayName("Books")]
        [BreadCrumb(Order = 1)]
        public IActionResult Books()
        {
            return View(viewName: "Index");
        }

        [DisplayName("Users")]
        [BreadCrumb(Order = 1)]
        public IActionResult Users()
        {
            return View(viewName: "Index");
        }

        [DisplayName("BooksGiven")]
        [BreadCrumb(Order = 1)]
        public IActionResult BooksGiven()
        {
            return View(viewName: "Index");
        }

        [DisplayName("BooksMissings")]
        [BreadCrumb(Order = 1)]
        public IActionResult BooksMissings()
        {
            return View(viewName: "Index");
        }
    }
}