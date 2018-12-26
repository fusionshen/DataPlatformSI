using System;
using DataPlatformSI.Common.GuardToolkit;
using DataPlatformSI.Common.IdentityToolkit;
using System.Threading.Tasks;
using DataPlatformSI.Entities.Identity;
using DataPlatformSI.Services.Contracts.Identity;
using DataPlatformSI.ViewModels.Identity.Settings;
using DNTCommon.Web.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DataPlatformSI.Services.Identity;
using DataPlatformSI.ViewModels.Identity;
using System.IO;
using DataPlatformSI.ViewModels.Identity.Emails;
using System.Security.Claims;
using DataPlatformSI.Services.Contracts;
using System.Collections.Generic;
using System.Linq;

namespace DataPlatformSI.WebAPI.Controllers
{
    /// <summary>
    /// 个人信息
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [EnableCors("CorsPolicy")]
    public class UserController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly IProtectionProviderService _protectionProviderService;
        private readonly IApplicationRoleManager _roleManager;
        private readonly IApplicationSignInManager _signInManager;
        private readonly IOptionsSnapshot<SiteSettings> _siteOptions;
        private readonly IUsedPasswordsService _usedPasswordsService;
        private readonly IApplicationUserManager _userManager;
        private readonly IUsersPhotoService _usersPhotoService;
        private readonly IUserValidator<User> _userValidator;
        private readonly ILogger<UserController> _logger;
        private readonly IModuleService _moduleService;

        public UserController(
            IApplicationUserManager userManager,
            IApplicationRoleManager roleManager,
            IApplicationSignInManager signInManager,
            IProtectionProviderService protectionProviderService,
            IUserValidator<User> userValidator,
            IUsedPasswordsService usedPasswordsService,
            IUsersPhotoService usersPhotoService,
            IOptionsSnapshot<SiteSettings> siteOptions,
            IEmailSender emailSender,
            ILogger<UserController> logger,
            IModuleService moduleService)
        {
            _userManager = userManager;
            _userManager.CheckArgumentIsNull(nameof(_userManager));

            _roleManager = roleManager;
            _roleManager.CheckArgumentIsNull(nameof(_roleManager));

            _signInManager = signInManager;
            _signInManager.CheckArgumentIsNull(nameof(_signInManager));

            _protectionProviderService = protectionProviderService;
            _protectionProviderService.CheckArgumentIsNull(nameof(_protectionProviderService));

            _userValidator = userValidator;
            _userValidator.CheckArgumentIsNull(nameof(_userValidator));

            _usedPasswordsService = usedPasswordsService;
            _usedPasswordsService.CheckArgumentIsNull(nameof(_usedPasswordsService));

            _usersPhotoService = usersPhotoService;
            _usersPhotoService.CheckArgumentIsNull(nameof(_usersPhotoService));

            _siteOptions = siteOptions;
            _siteOptions.CheckArgumentIsNull(nameof(_siteOptions));

            _emailSender = emailSender;
            _emailSender.CheckArgumentIsNull(nameof(_emailSender));

            _logger = logger;
            _logger.CheckArgumentIsNull(nameof(_logger));

            _moduleService = moduleService;
            _moduleService.CheckArgumentIsNull(nameof(_moduleService));
        }

        /// <summary>
        /// 获取个人基本信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var user = await _userManager.GetCurrentUserAsync();
            var claimsIdentity = User.Identity as ClaimsIdentity;
            if (claimsIdentity.HasClaim(ClaimTypes.Role, ConstantRoles.Admin))
            {
                return Json(new { user.Id, Username = claimsIdentity.Name, Apps = (await _moduleService.GetAllModulesAsync()).Where(module => module.IsCore.HasValue).Select(module => module.Id).ToList(), Roles = new List<string> { ConstantRoles.Admin }, Info = await RenderModel(user, isAdminEdit: true) });
            }
            else
            {
                var roles = await _roleManager.GetRolesForUserAsync(user.Id);

                return Json(new { user.Id, Username = claimsIdentity.Name,Apps = (await _moduleService.GetAllModulesAsync()).Where(module => roles.Any(r => r.Claims.Any(c => c.ClaimValue.Contains(module.SpaceName)))).Select(module => module.Id).ToList(),Roles = roles.Select(r => r.Name).ToList(), Info = await RenderModel(user, isAdminEdit: false) });
            }
        }

        /// <summary>
        /// 更改个人信息
        /// </summary>
        /// <param name="model">更改信息所需</param>
        /// <returns>期望返回</returns>
        [HttpPut]
        public async Task<IActionResult> Put(UserProfileViewModel model)
        {
            var pid = _protectionProviderService.Decrypt(model.Pid);
            if (string.IsNullOrWhiteSpace(pid))
            {
                return BadRequest("Error");
            }

            if (pid != _userManager.GetCurrentUserId() &&
                ! await _roleManager.IsCurrentUserInRoleAsync(ConstantRoles.Admin))
            {
                _logger.LogWarning($"不存在用户{pid}");
                return BadRequest("Error");
            }

            var user = await _userManager.FindByIdAsync(pid);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.IsEmailPublic = model.IsEmailPublic;
            user.TwoFactorEnabled = model.TwoFactorEnabled;
            user.Location = model.Location;

            UpdateUserBirthDate(model, user);

            var (can, error) = await CanUpdateUserName(model, user);
            if (!can)
            {
                return BadRequest(error);
            }
            (can,error) = await CanUpdateUserAvatarImage(model, user);
            if (!can)
            {
                return BadRequest(error);
            }
            (can, error) = await CanUpdateUserEmail(model, user);
            if (!can)
            {
                return BadRequest(error);
            }
            var updateResult = await _userManager.UpdateAsync(user);
            if (updateResult.Succeeded)
            {
                if (!model.IsAdminEdit)
                {
                    // reflect the changes in the current user's Identity cookie
                    await _signInManager.RefreshSignInAsync(user);
                }

                await _emailSender.SendEmailAsync(
                        email: user.Email,
                        subject: "用户个人信息更改通知",
                        viewNameOrPath: "~/Areas/Identity/Views/EmailTemplates/_UserProfileUpdateNotification.cshtml",
                        model: new UserProfileUpdateNotificationViewModel
                        {
                            User = user,
                            EmailSignature = _siteOptions.Value.Smtp.FromName,
                            MessageDateTime = DateTime.UtcNow.ToLocalTime().ToString()
                        });

                //return RedirectToAction(nameof(Index), "UserCard", routeValues: new { id = user.Id });
                return Ok();
            }
            return BadRequest(updateResult.DumpErrors(useHtmlNewLine: true));
        }

        /// <summary>
        /// 验证用户名和邮箱
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="email">邮箱</param>
        /// <param name="pid">加密的userId</param>
        /// <returns></returns>
        [HttpPost("[action]")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> ValidateUsername(string username, string email, string pid)
        {
            pid = _protectionProviderService.Decrypt(pid);
            if (string.IsNullOrWhiteSpace(pid))
            {
                return Json("缺少参数");
            }

            var user = await _userManager.FindByIdAsync(pid);
            user.UserName = username;
            user.Email = email;

            var result = await _userValidator.ValidateAsync((UserManager<User>)_userManager, user);
            return Json(result.Succeeded ? "true" : result.DumpErrors(useHtmlNewLine: true));
        }

        private void UpdateUserBirthDate(UserProfileViewModel model, User user)
        {
            if (model.DateOfBirthYear.HasValue &&
                model.DateOfBirthMonth.HasValue &&
                model.DateOfBirthDay.HasValue)
            {
                var date =
                    $"{model.DateOfBirthYear.Value.ToString()}/{model.DateOfBirthMonth.Value.ToString("00")}/{model.DateOfBirthDay.Value.ToString("00")}";
                user.BirthDate = Convert.ToDateTime(date);
            }
            else
            {
                user.BirthDate = null;
            }
        }

        private async Task<UserProfileViewModel> RenderModel(User user, bool isAdminEdit)
        {
            _usersPhotoService.SetUserDefaultPhoto(user);

            var userProfile = new UserProfileViewModel
            {
                IsAdminEdit = isAdminEdit,
                Email = user.Email,
                PhotoFileName = user.PhotoFileName,
                Location = user.Location,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Pid = _protectionProviderService.Encrypt(user.Id.ToString()),
                IsEmailPublic = user.IsEmailPublic,
                TwoFactorEnabled = user.TwoFactorEnabled,
                IsPasswordTooOld = await _usedPasswordsService.IsLastUserPasswordTooOldAsync(user.Id)
            };

            if (user.BirthDate.HasValue)
            {
                userProfile.DateOfBirthYear = user.BirthDate.Value.Year;
                userProfile.DateOfBirthMonth = user.BirthDate.Value.Month;
                userProfile.DateOfBirthDay = user.BirthDate.Value.Day;
            }

            return userProfile;
        }

        private async Task<(bool,string)> CanUpdateUserAvatarImage(UserProfileViewModel model, User user)
        {
            _usersPhotoService.SetUserDefaultPhoto(user);

            var photoFile = model.Photo;
            if (photoFile != null && photoFile.Length > 0)
            {
                var imageOptions = _siteOptions.Value.UserAvatarImageOptions;
                if (!photoFile.IsValidImageFile(maxWidth: imageOptions.MaxWidth, maxHeight: imageOptions.MaxHeight))
                {
                    return (false, $"图片最大高度为{imageOptions.MaxHeight} ，最大宽度为{imageOptions.MaxWidth}");
                }

                var uploadsRootFolder = _usersPhotoService.GetUsersAvatarsFolderPath();
                var photoFileName = $"{user.Id}{Path.GetExtension(photoFile.FileName)}";
                var filePath = Path.Combine(uploadsRootFolder, photoFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await photoFile.CopyToAsync(fileStream);
                }
                user.PhotoFileName = photoFileName;
            }
            return (true,null);
        }

        private async Task<(bool,string)> CanUpdateUserEmail(UserProfileViewModel model, User user)
        {
            if (user.Email != model.Email)
            {
                user.Email = model.Email;
                var result =
                    await _userValidator.ValidateAsync((UserManager<User>)_userManager, user);
                if (!result.Succeeded)
                {
                    return (false,result.DumpErrors(useHtmlNewLine:true));
                }

                user.EmailConfirmed = false;

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                await _emailSender.SendEmailAsync(
                    email: user.Email,
                    subject: "更改邮箱确认",
                    viewNameOrPath: "~/Areas/Identity/Views/EmailTemplates/_RegisterEmailConfirmation.cshtml",
                    model: new RegisterEmailConfirmationViewModel
                    {
                        User = user,
                        EmailConfirmationToken = code,
                        EmailSignature = _siteOptions.Value.Smtp.FromName,
                        MessageDateTime = DateTime.UtcNow.ToLocalTime().ToString()
                    });
            }

            return (true,null);
        }

        private async Task<(bool,string)> CanUpdateUserName(UserProfileViewModel model, User user)
        {
            if (user.UserName != model.UserName)
            {
                user.UserName = model.UserName;
                var result =
                    await _userValidator.ValidateAsync((UserManager<User>)_userManager, user);
                return (result.Succeeded,result.DumpErrors(useHtmlNewLine: true));
            }
            return (true, null);
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="model">修改密码所需信息</param>
        /// <returns>是否修改成功</returns>
        [HttpPost("[action]")]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var user = await _userManager.GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _userManager.UpdateSecurityStampAsync(user);

                // reflect the changes in the Identity cookie
                await _signInManager.RefreshSignInAsync(user);

                await _emailSender.SendEmailAsync(
                           email: user.Email,
                           subject: "您的密码已重置",
                           viewNameOrPath: "~/Areas/Identity/Views/EmailTemplates/_ChangePasswordNotification.cshtml",
                           model: new ChangePasswordNotificationViewModel
                           {
                               User = user,
                               EmailSignature = _siteOptions.Value.Smtp.FromName,
                               MessageDateTime = DateTime.UtcNow.ToLocalTime().ToString()
                           });
                return Ok();
            }

            return Json(result.DumpErrors(useHtmlNewLine: false));
        }

    }
}
