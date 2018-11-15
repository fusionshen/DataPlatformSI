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
    [DisplayName("کنترلر آزمایشی با سطح دسترسی پویا")]
    // [NoBrowserCache]
    public class DynamicPermissionsTestController : Controller
    {
        [DisplayName("ایندکس")]
        [BreadCrumb(Order = 1)]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost] // More info: http://www.dotnettips.info/post/2468/ and http://www.dotnettips.info/post/2470/
        public IActionResult Index([FromBody]RoleViewModel model)
        {
            return Json(model);
        }

        [DisplayName("گزارش از لیست محصولات")]
        [BreadCrumb(Order = 1)]
        public IActionResult Products()
        {
            return View(viewName: "Index");
        }

        [DisplayName("گزارش از لیست سفارشات")]
        [BreadCrumb(Order = 1)]
        public IActionResult Orders()
        {
            return View(viewName: "Index");
        }

        [DisplayName("گزارش از لیست فروش")]
        [BreadCrumb(Order = 1)]
        public IActionResult Sells()
        {
            return View(viewName: "Index");
        }

        [DisplayName("گزارش از لیست خریداران")]
        [BreadCrumb(Order = 1)]
        public IActionResult Customers()
        {
            return View(viewName: "Index");
        }
    }
}
