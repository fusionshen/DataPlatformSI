using DataPlatformSI.DataLayer.Context;
using DataPlatformSI.Common.IdentityToolkit;
using DataPlatformSI.Common.GuardToolkit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using DataPlatformSI.Services.Identity;
using DataPlatformSI.Services.Contracts.Identity;
using DataPlatformSI.Entities.Identity;
using DataPlatformSI.ViewModels.Identity;
using Microsoft.AspNetCore.Identity;
using DataPlatformSI.ViewModels.Identity.Emails;
using System;
using Microsoft.Extensions.Options;
using DataPlatformSI.ViewModels.Identity.Settings;
using Microsoft.Extensions.Logging;

namespace DataPlatformSI.WebAPI.Controllers
{
    /// <summary>
    /// 个人账户
    /// </summary>
    [Route("api/[controller]")]
    [EnableCors("CorsPolicy")]
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IApplicationUserManager _userManager;
        private readonly IApplicationSignInManager _signInManager;
        private readonly IPasswordValidator<User> _passwordValidator;
        private readonly IUserValidator<User> _userValidator;
        private readonly ITokenStoreService _tokenStoreService;
        private readonly IUnitOfWork _uow;
        private readonly IAntiForgeryCookieService _antiforgery;
        private readonly IOptionsSnapshot<SiteSettings> _siteOptions;

        public AccountController(
            IApplicationUserManager userManager,
            IApplicationSignInManager signInManager,
            IEmailSender emailSender,
            IPasswordValidator<User> passwordValidator,
            IUserValidator<User> userValidator,
            ITokenStoreService tokenStoreService,
            IUnitOfWork uow,
            IAntiForgeryCookieService antiforgery,
            IOptionsSnapshot<SiteSettings> siteOptions,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _userManager.CheckArgumentIsNull(nameof(userManager));

            _signInManager = signInManager;
            _signInManager.CheckArgumentIsNull(nameof(signInManager));

            _tokenStoreService = tokenStoreService;
            _tokenStoreService.CheckArgumentIsNull(nameof(tokenStoreService));

            _passwordValidator = passwordValidator;
            _passwordValidator.CheckArgumentIsNull(nameof(_passwordValidator));

            _userValidator = userValidator;
            _userValidator.CheckArgumentIsNull(nameof(_userValidator));

            _emailSender = emailSender;
            _emailSender.CheckArgumentIsNull(nameof(_emailSender));

            _uow = uow;
            _uow.CheckArgumentIsNull(nameof(_uow));

            _antiforgery = antiforgery;
            _antiforgery.CheckArgumentIsNull(nameof(antiforgery));

            _siteOptions = siteOptions;
            _siteOptions.CheckArgumentIsNull(nameof(_siteOptions));

            _logger = logger;
            _logger.CheckArgumentIsNull(nameof(_logger));
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="loginUser">登录所需信息</param>
        /// <param name="returnUrl">跳转的路由</param>
        /// <returns>token与其过期时间</returns>
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromBody]  LoginViewModel loginUser, string returnUrl = null)
        {
            var user = await _userManager.FindByNameAsync(loginUser.Username);
            if (user == null)
            {
                return NotFound();
            }

            if (!user.IsActive)
            {
                return Json("用户未被激活");
            }

            if (_siteOptions.Value.EnableEmailConfirmation &&
                !await _userManager.IsEmailConfirmedAsync(user))
            {
                return Json("新用户未被邮件确认");
            }

            var result = await _signInManager.PasswordSignInAsync(
                                    loginUser.Username,
                                    loginUser.Password,
                                    loginUser.RememberMe,
                                    lockoutOnFailure: true);
            if (result.Succeeded)
            {
                _logger.LogInformation(1, $"{loginUser.Username} logged in.");

                var (accessToken, accessTokenExpire,refreshToken,refreshTokenExpire, claims) = await _tokenStoreService.CreateJwtTokens(user, refreshTokenSource: null);

                _antiforgery.RegenerateAntiForgeryCookies(claims);

                return Ok(new { access_token = accessToken, access_token_expire_time = accessTokenExpire, refresh_token = refreshToken, refresh_token_expire_time = refreshTokenExpire });
            }

            if (result.RequiresTwoFactor)
            {
                return RedirectToAction(
                        nameof(TwoFactorController.SendCode),
                        "TwoFactor",
                        routeValues: new { ReturnUrl = returnUrl, loginUser.RememberMe });
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning(2, $"用户:{loginUser.Username}被锁定");
                return Json("用户被锁定");

            }

            if (result.IsNotAllowed)
            {
                return Json("用户被禁");
            }

            return BadRequest("发生一些莫名错误");
        }

        /// <summary>
        /// 用户登出
        /// </summary>
        /// <param name="refreshToken">用于刷新的token</param>
        /// <returns>是否登出</returns>
        [AllowAnonymous]
        [HttpGet("[action]")]
        public async Task<IActionResult> Logout(string refreshToken)
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var userIdValue = claimsIdentity.FindFirst(ClaimTypes.UserData)?.Value;

            var user = User.Identity.IsAuthenticated ? await _userManager.FindByIdAsync(userIdValue) : null;
            await _signInManager.SignOutAsync();
            if (user != null)
            {
                await _userManager.UpdateSecurityStampAsync(user);
                // The Jwt implementation does not support "revoke OAuth token" (logout) by design.
                // Delete the user's tokens from the database (revoke its bearer token)
                await _tokenStoreService.RevokeUserBearerTokensAsync(userIdValue, refreshToken);
                await _uow.SaveChangesAsync();
                _antiforgery.DeleteAntiForgeryCookies();
                _logger.LogInformation(4, $"{user.UserName} logged out.");
                return Json("true");
            }
            return BadRequest("user not found");
        }

        /// <summary>
        /// 刷新jwt
        /// </summary>
        /// <param name="jsonBody">刷新token所需信息</param>
        /// <returns>token与其过期时间</returns>
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        [HttpPost("[action]")]
        public async Task<IActionResult> RefreshToken([FromBody]JToken jsonBody)
        {
            var refreshToken = jsonBody.Value<string>("refreshToken");
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return BadRequest("refreshToken is not set.");
            }

            var token = await _tokenStoreService.FindTokenAsync(refreshToken);
            if (token == null)
            {
                return Unauthorized();
            }

            var (accessToken, accessTokenExpire, newRefreshToken, refreshTokenExpire, claims) = await _tokenStoreService.CreateJwtTokens(token.User, refreshToken);

            _antiforgery.RegenerateAntiForgeryCookies(claims);

            return Ok(new { access_token = accessToken, access_token_expire_time = accessTokenExpire, refresh_token = newRefreshToken, refresh_token_expire_time = refreshTokenExpire });
        }

       

        /// <summary>
        /// 验证密码
        /// </summary>
        /// <param name="newPassword">新密码</param>
        /// <returns>是否成功</returns>
        [Authorize]
        [HttpPost("[action]"), ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> ValidatePassword(string newPassword)
        {
            var user = await _userManager.GetCurrentUserAsync();
            var result = await _passwordValidator.ValidateAsync(
                (UserManager<User>)_userManager, user, newPassword);
            return Json(result.Succeeded ? "true" : result.DumpErrors(useHtmlNewLine: true));
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="model">修改密码所需信息</param>
        /// <returns>是否修改成功</returns>
        [Authorize]
        [HttpPost("[action]"), ValidateAntiForgeryToken]
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

            return Json(result.DumpErrors(useHtmlNewLine: true));
        }

        /// <summary>
        /// 用户信息 For Test
        /// </summary>
        /// <returns>用户信息</returns>
        [Authorize]
        [HttpGet("[action]"), HttpPost("[action]")]
        public IActionResult GetUserInfo()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            return Json(new { Username = claimsIdentity.Name, Apps = new List<int> { 1 ,2}, Roles = new List<string> { ConstantRoles.Admin } });
        }

        /// <summary>
        /// 邮箱验证密码
        /// </summary>
        /// <param name="password">账户密码</param>
        /// <param name="email">账户邮箱</param>
        /// <returns>验证结果</returns>
        [AllowAnonymous]
        [HttpPost("[action]"), ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> ValidatePasswordByEmail(string password, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest("该邮箱并未进行注册");
            }

            var result = await _passwordValidator.ValidateAsync(
                (UserManager<User>)_userManager, user, password);
            return Json(result.Succeeded ? "true" : result.DumpErrors(useHtmlNewLine: true));
        }

        /// <summary>
        /// 忘记密码
        /// </summary>
        /// <param name="model">邮箱</param>
        /// <returns>发送密码重置邮件</returns>
        [AllowAnonymous]
        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
            {
                return Json("该邮箱并未进行注册");
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _emailSender.SendEmailAsync(
                email: model.Email,
                subject: "密码重置",
                viewNameOrPath: "~/Areas/Identity/Views/EmailTemplates/_PasswordReset.cshtml",
                model: new PasswordResetViewModel
                {
                    UserId = user.Id,
                    Token = code,
                    EmailSignature = _siteOptions.Value.Smtp.FromName,
                    MessageDateTime = DateTime.UtcNow.ToLocalTime().ToString()
                })
                ;

            return Json("ForgotPasswordConfirmation");
        }

        /// <summary>
        /// 重置密码
        /// </summary>
        /// <param name="model">重置密码所需信息</param>
        /// <returns>重置结果</returns>
        [AllowAnonymous]
        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return Json("该邮箱并未进行注册");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            return Json(result.Succeeded ? "true" : result.DumpErrors(useHtmlNewLine: true));
        }

        /// <summary>
        /// 验证账户用户名和邮箱
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="email">邮箱</param>
        /// <returns>期望返回</returns>
        [AllowAnonymous]
        [HttpPost("[action]"), ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> ValidateUsername(string username, string email)
        {
            var result = await _userValidator.ValidateAsync(
                (UserManager<User>)_userManager, new User { UserName = username, Email = email });
            return Json(result.Succeeded ? "true" : result.DumpErrors(useHtmlNewLine: true));
        }

        /// <summary>
        /// 根据用户名验证密码
        /// </summary>
        /// <param name="password">密码</param>
        /// <param name="username">用户名</param>
        /// <returns>期待返回</returns>
        [AllowAnonymous]
        [HttpPost("[action]"), ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> ValidatePasswordByUsername(string password, string username)
        {
            var result = await _passwordValidator.ValidateAsync(
                (UserManager<User>)_userManager, new User { UserName = username }, password);
            return Json(result.Succeeded ? "true" : result.DumpErrors(useHtmlNewLine: true));
        }

        /// <summary>
        /// 邮件确认验证码
        /// </summary>
        /// <param name="userId">用户密码</param>
        /// <param name="code">验证码</param>
        /// <returns>期望返回</returns>
        [AllowAnonymous]
        [HttpPost("[action]"), ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return BadRequest("Error");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest("NotFound");
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);
            return Json(result.Succeeded ? "true" : result.DumpErrors(useHtmlNewLine: true));
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="model">注册所需信息</param>
        /// <returns>期望返回</returns>
        [AllowAnonymous]
        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            var user = new User
            {
                UserName = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation(3, $"{user.UserName} created a new account with password.");

                if (_siteOptions.Value.EnableEmailConfirmation)
                {
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //ControllerExtensions.ShortControllerName<RegisterController>(), //todo: use everywhere .................

                    await _emailSender.SendEmailAsync(
                        email: user.Email,
                        subject: "请激活您的账户",
                        viewNameOrPath: "~/Areas/Identity/Views/EmailTemplates/_RegisterEmailConfirmation.cshtml",
                        model: new RegisterEmailConfirmationViewModel
                        {
                            User = user,
                            EmailConfirmationToken = code,
                            EmailSignature = _siteOptions.Value.Smtp.FromName,
                            MessageDateTime = DateTime.UtcNow.ToLocalTime().ToString()
                        });
                }
            }

            return Json(result.Succeeded ? "true" : result.DumpErrors(useHtmlNewLine: true));

        }
    }
}