using DataPlatformSI.Common.GuardToolkit;
using DataPlatformSI.Common.IdentityToolkit;
using System.Threading.Tasks;
using DataPlatformSI.Services.Contracts.Identity;
using DataPlatformSI.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DataPlatformSI.Entities.Identity;
using System.Data.SqlClient;
using DataPlatformSI.ViewModels.Identity;

namespace DataPlatformSI.WebAPI.Controllers
{
    /// <summary>
    /// 用户管理
    /// </summary>
    [Authorize(Roles = ConstantRoles.Admin)]
    [Route("api/[controller]")]
    [EnableCors("CorsPolicy")]
    public class UsersController : Controller
    {
        private const int DefaultPageSize = 7;

        private readonly IApplicationRoleManager _roleManager;
        private readonly IApplicationUserManager _userManager;
        public UsersController(
            IApplicationUserManager userManager,
            IApplicationRoleManager roleManager)
        {
            _userManager = userManager;
            _userManager.CheckArgumentIsNull(nameof(_userManager));

            _roleManager = roleManager;
            _roleManager.CheckArgumentIsNull(nameof(_roleManager));
        }

        /// <summary>
        /// 获取分页列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="field"></param>
        /// <param name="order"></param>
        /// <returns>期望返回</returns>
        [HttpGet]
        public async Task<IActionResult> Get(int? page = 1, string field = "Id", SortOrder order = SortOrder.Ascending)
        {
            var model = await _userManager.GetPagedUsersListAsync(
                pageNumber: page.Value - 1,
                recordsPerPage: DefaultPageSize,
                sortByField: field,
                sortOrder: order,
                showAllUsers: true);

            model.Paging.CurrentPage = page.Value;
            model.Paging.ItemsPerPage = DefaultPageSize;
            model.Paging.ShowFirstLast = true;
            return Json(model);
        }

        /// <summary>
        /// 获取所有用户列表
        /// </summary>
        /// <returns>期望返回</returns>
        [HttpGet("[action]")]
        public async Task<IActionResult> GetUserList()
        {
            return Json(await _userManager.GetUserListAsync());
        }


        /// <summary>
        /// 激活邮件确认
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <returns>期望输出</returns>
        [HttpPost("{userId}/[action]")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> ActivateUserEmailStat(int userId)
        {
            User thisUser = null;
            var result = await _userManager.UpdateUserAndSecurityStampAsync(
                userId, user =>
                {
                    user.EmailConfirmed = true;
                    thisUser = user;
                });
            return result.Succeeded ? await ReturnUserCard(thisUser) : BadRequest(error: result.DumpErrors(useHtmlNewLine: true));
        }

        /// <summary>
        /// 修改封停状态
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="activate">是否封停</param>
        /// <returns>期望返回</returns>
        [HttpPost("{userId}/[action]")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> ChangeUserLockoutMode(int userId, bool activate)
        {
            User thisUser = null;
            var result = await _userManager.UpdateUserAndSecurityStampAsync(
                userId, user =>
                {
                    user.LockoutEnabled = activate;
                    thisUser = user;
                });
            return result.Succeeded ? await ReturnUserCard(thisUser) : BadRequest(error: result.DumpErrors(useHtmlNewLine: true));
        }

        /// <summary>
        /// 修改用户角色
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="roleIds">角色Ids</param>
        /// <returns>期望返回</returns>
        [HttpPost("{userId}/[action]")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> ChangeUserRoles(int userId, int[] roleIds)
        {
            User thisUser = null;
            var result = await _userManager.AddOrUpdateUserRolesAsync(
                userId, roleIds, user => thisUser = user);
            return result.Succeeded ? await ReturnUserCard(thisUser) : BadRequest(error: result.DumpErrors(useHtmlNewLine: true));
        }

        /// <summary>
        /// 激活或失活
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="activate">是否激活</param>
        /// <returns>期望返回</returns>
        [HttpPost("{userId}/[action]")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> ChangeUserStat(int userId, bool activate)
        {
            User thisUser = null;
            var result = await _userManager.UpdateUserAndSecurityStampAsync(
                userId, user =>
                {
                    user.IsActive = activate;
                    thisUser = user;
                });
            return result.Succeeded ? await ReturnUserCard(thisUser) : BadRequest(error: result.DumpErrors(useHtmlNewLine: true));
        }

        /// <summary>
        /// 修改用户两步登录状态
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="activate">打开或者关闭</param>
        /// <returns>期望返回</returns>
        [HttpPost("{userId}/[action]")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> ChangeUserTwoFactorAuthenticationStat(int userId, bool activate)
        {
            User thisUser = null;
            var result = await _userManager.UpdateUserAndSecurityStampAsync(
                userId, user =>
                {
                    user.TwoFactorEnabled = activate;
                    thisUser = user;
                });
            return result.Succeeded ? await ReturnUserCard(thisUser) : BadRequest(error: result.DumpErrors(useHtmlNewLine: true));
        }

        /// <summary>
        /// 清除封停时间
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <returns>期望返回</returns>
        [HttpPost("{userId}/[action]")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> EndUserLockout(int userId)
        {
            User thisUser = null;
            var result = await _userManager.UpdateUserAndSecurityStampAsync(
                userId, user =>
                {
                    user.LockoutEnd = null;
                    thisUser = user;
                });
            return result.Succeeded ? await ReturnUserCard(thisUser) : BadRequest(error: result.DumpErrors(useHtmlNewLine: true));
        }

        /// <summary>
        /// 模糊查找
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> SearchUsers(SearchUsersViewModel model)
        {
            var pagedUsersList = await _userManager.GetPagedUsersListAsync(
                pageNumber: 0,
                model: model);

            pagedUsersList.Paging.CurrentPage = 1;
            pagedUsersList.Paging.ItemsPerPage = model.MaxNumberOfRows;
            pagedUsersList.Paging.ShowFirstLast = true;

            model.PagedUsersList = pagedUsersList;
            return Json(model);
        }

        private async Task<IActionResult> ReturnUserCard(User thisUser)
        {
            var roles = await _roleManager.GetAllCustomRolesAsync();
            return Json(
                new UserCardItemViewModel
                {
                    User = thisUser,
                    ShowAdminParts = true,
                    Roles = roles,
                    ActiveTab = UserCardItemActiveTab.UserAdmin
                });
        }

        /// <summary>
        /// 获得用户卡片
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <returns>期望返回</returns>
        [AllowAnonymous]
        [HttpGet("{userId}")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Get(int? userId)
        {
            if (!userId.HasValue && User.Identity.IsAuthenticated)
            {
                userId = User.Identity.GetUserId<int>();
            }

            if (!userId.HasValue)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdIncludeUserRolesAsync(userId.Value);
            if (user == null)
            {
                return NotFound();
            }

            var model = new UserCardItemViewModel
            {
                User = user,
                ShowAdminParts = User.IsInRole(ConstantRoles.Admin),
                Roles = await _roleManager.GetAllCustomRolesAsync(),
                ActiveTab = UserCardItemActiveTab.UserInfo
            };
            return Json(model);
        }

        /// <summary>
        /// 获得用户邮箱图片
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <returns>期望返回</returns>
        [AllowAnonymous]
        [HttpGet("{userId}/[action]")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> EmailToImage(int? userId)
        {
            if (!userId.HasValue)
            {
                return NotFound();
            }

            var fileContents = await _userManager.GetEmailImageAsync(userId);
            return new FileContentResult(fileContents, "image/png");
        }
    }
}