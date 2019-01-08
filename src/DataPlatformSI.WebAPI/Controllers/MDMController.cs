using DataPlatformSI.Services.Identity;
using DataPlatformSI.ViewModels.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace DataPlatformSI.WebAPI.Controllers
{
    [Authorize(Policy = ConstantPolicies.DynamicPermission)]
    [Area("DataPlatformRI.Modules.MDM")]
    [DisplayName("主数据管理模块")]
    public class MDMController : Controller
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