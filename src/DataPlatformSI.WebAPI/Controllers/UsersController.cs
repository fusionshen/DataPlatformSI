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
using System;
using System.ComponentModel;
using Microsoft.Extensions.Options;
using DataPlatformSI.ViewModels.Identity.Settings;
using DataPlatformSI.ViewModels.Identity.Emails;

namespace DataPlatformSI.WebAPI.Controllers
{
    /// <summary>
    /// 用户管理
    /// </summary>
    [Authorize(Policy = ConstantPolicies.DynamicPermission)]
    [Area("DataPlatformRI.Modules.Users")]
    [DisplayName("用户模块")]
    [Route("api/[controller]")]
    [EnableCors("CorsPolicy")]
    public class UsersController : Controller
    {
        private const int DefaultPageSize = 7;

        private readonly IApplicationRoleManager _roleManager;
        private readonly IApplicationUserManager _userManager;
        private readonly IApplicationSignInManager _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IOptionsSnapshot<SiteSettings> _siteOptions;
        public UsersController(
            IApplicationUserManager userManager,
            IApplicationRoleManager roleManager,
            IApplicationSignInManager signInManager,
            IOptionsSnapshot<SiteSettings> siteOptions)
        {
            _userManager = userManager;
            _userManager.CheckArgumentIsNull(nameof(_userManager));

            _roleManager = roleManager;
            _roleManager.CheckArgumentIsNull(nameof(_roleManager));

            _signInManager = signInManager;
            _signInManager.CheckArgumentIsNull(nameof(_signInManager));

            _siteOptions = siteOptions;
            _siteOptions.CheckArgumentIsNull(nameof(_siteOptions));
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
        [DisplayName("获取用户列表")]
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
            return result.Succeeded ? await ReturnUserCard(thisUser) : BadRequest(error: result.DumpErrors(useHtmlNewLine: false));
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
            return result.Succeeded ? await ReturnUserCard(thisUser) : BadRequest(error: result.DumpErrors(useHtmlNewLine: false));
        }

        /// <summary>
        /// 激活或失活
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="activate">是否激活</param>
        /// <returns>期望返回</returns>
        [DisplayName("激活或失活用户")]
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
            return result.Succeeded ? await ReturnUserCard(thisUser) : BadRequest(error: result.DumpErrors(useHtmlNewLine: false));
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
            return result.Succeeded ? await ReturnUserCard(thisUser) : BadRequest(error: result.DumpErrors(useHtmlNewLine: false));
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
            return result.Succeeded ? await ReturnUserCard(thisUser) : BadRequest(error: result.DumpErrors(useHtmlNewLine: false));
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
        public async Task<IActionResult> Get(int userId)
        {
            //if (!userId.HasValue && User.Identity.IsAuthenticated)
            //{
            //    userId = User.Identity.GetUserId<int>();
            //}

            //if (!userId.HasValue)
            //{
            //    return NotFound();
            //}

            var user = await _userManager.FindByIdIncludeUserRolesAsync(userId);
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


        /// <summary>
        /// 新增User
        /// </summary>
        /// <param name="model">新增所需</param>
        /// <returns>期望返回</returns>
        [DisplayName("新增用户")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.DumpErrors(useHtmlNewLine: false));
            }
            var user = new User
            {
                UserName = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                BirthDate = model.BirthDate,
                Location = model.Location
            };
            var result = await _userManager.CreateAsync(user, "safepass");
            if (!result.Succeeded)
            {
                return BadRequest(result.DumpErrors(useHtmlNewLine: false));
            }
            if (model.RoleIds.Length !=0 )
            {
                result = await _userManager.AddOrUpdateUserRolesAsync(user.Id, model.RoleIds, u => user = u);
                if (!result.Succeeded)
                {
                    return BadRequest(error: result.DumpErrors(useHtmlNewLine: false));
                }
            }
            //return result.Succeeded ? await ReturnUserCard(user) : BadRequest(error: result.DumpErrors(useHtmlNewLine: true));
            return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
        }

        /// <summary>
        /// 管理员修改用户
        /// </summary>
        /// <param name="id">用户Id</param>
        /// <param name="model">用户所需</param>
        /// <returns>期望返回</returns>
        // PUT: api/Users/5
        [DisplayName("修改用户")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.DumpErrors(false));
            }
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }
            if (user.NormalizedUserName != "ADMIN")
            {
                user.UserName = model.Username;
            }
            user.Email = model.Email;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.BirthDate = model.BirthDate;
            user.Location = model.Location;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.DumpErrors(useHtmlNewLine: false));
            }
            if (user.NormalizedUserName != "ADMIN")
            {
                result = await _userManager.AddOrUpdateUserRolesAsync(user.Id, model.RoleIds, u => user = u);
                if (!result.Succeeded)
                {
                    return BadRequest(error: result.DumpErrors(useHtmlNewLine: false));
                }
            }
            return Json(user);
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="id">用户id</param>
        /// <returns></returns>
        [DisplayName("删除用户")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound("user is not found");
            }
            if (user.NormalizedUserName == "ADMIN")
            {
                return BadRequest("USER ADMIN CANT BE REMOVED");
            }
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.DumpErrors(useHtmlNewLine: false));
            }
            return NoContent();
        }

        /// <summary>
        /// 重置密码
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <returns></returns>
        [DisplayName("重置密码")]
        [HttpGet("{userId}/[action]")]
        public async Task<IActionResult> ResetPassword(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return NotFound();
            }
            if (user.NormalizedUserName == "ADMIN")
            {
                return BadRequest("USER ADMIN CANT BE RESET");
            }
            var result = await _userManager.ResetPasswordAsync(user, await _userManager.GeneratePasswordResetTokenAsync(user), "safepass");
            if (result.Succeeded)
            {
                await _userManager.UpdateSecurityStampAsync(user);

                // reflect the changes in the Identity cookie
                await _signInManager.RefreshSignInAsync(user);

                //await _emailSender.SendEmailAsync(
                //           email: user.Email,
                //           subject: "您的密码已重置",
                //           viewNameOrPath: "~/Areas/Identity/Views/EmailTemplates/_ChangePasswordNotification.cshtml",
                //           model: new ChangePasswordNotificationViewModel
                //           {
                //               User = user,
                //               EmailSignature = _siteOptions.Value.Smtp.FromName,
                //               MessageDateTime = DateTime.UtcNow.ToLocalTime().ToString()
                //           });
                return Ok();
            }

            return BadRequest(result.DumpErrors(useHtmlNewLine: false));
        }


    }
}