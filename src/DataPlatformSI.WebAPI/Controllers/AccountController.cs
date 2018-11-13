using DataPlatformSI.DataLayer.Context;
using DataPlatformSI.Common.IdentityToolkit;
using DataPlatformSI.Common.GuardToolkit;
using DataPlatformSI.Services;
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
using System.ComponentModel;
using DataPlatformSI.Services.Authorization;
using Microsoft.AspNetCore.Identity;
using DataPlatformSI.ViewModels.Identity.Emails;
using System;
using Microsoft.Extensions.Options;
using DataPlatformSI.ViewModels.Identity.Settings;

namespace DataPlatformSI.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("CorsPolicy")]
    public class AccountController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly IApplicationUserManager _userManager;
        private readonly IApplicationSignInManager _signInManager;
        private readonly IPasswordValidator<User> _passwordValidator;
        private readonly ITokenStoreService _tokenStoreService;
        private readonly IUnitOfWork _uow;
        private readonly IAntiForgeryCookieService _antiforgery;
        private readonly IOptionsSnapshot<SiteSettings> _siteOptions;

        public AccountController(
            IApplicationUserManager userManager,
            IApplicationSignInManager signInManager,
            IEmailSender emailSender,
            IPasswordValidator<User> passwordValidator,
            ITokenStoreService tokenStoreService,
            IUnitOfWork uow,
            IAntiForgeryCookieService antiforgery,
            IOptionsSnapshot<SiteSettings> siteOptions)
        {
            _userManager = userManager;
            _userManager.CheckArgumentIsNull(nameof(userManager));

            _signInManager = signInManager;
            _signInManager.CheckArgumentIsNull(nameof(signInManager));

            _tokenStoreService = tokenStoreService;
            _tokenStoreService.CheckArgumentIsNull(nameof(tokenStoreService));

            _passwordValidator = passwordValidator;
            _passwordValidator.CheckArgumentIsNull(nameof(_passwordValidator));

            _emailSender = emailSender;
            _emailSender.CheckArgumentIsNull(nameof(_emailSender));

            _uow = uow;
            _uow.CheckArgumentIsNull(nameof(_uow));

            _antiforgery = antiforgery;
            _antiforgery.CheckArgumentIsNull(nameof(antiforgery));

            _siteOptions = siteOptions;
            _siteOptions.CheckArgumentIsNull(nameof(_siteOptions));
        }

        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromBody]  LoginViewModel loginUser)
        {
            if (loginUser == null)
            {
                return BadRequest("user is not set.");
            }

            var user = await _userManager.FindByNameAsync(loginUser.Username);
            if (user == null)
            {
                return Unauthorized();
            }

            if (!user.IsActive)
            {
                return Unauthorized();
            }

            //if (_siteOptions.Value.EnableEmailConfirmation &&
            //    !await _userManager.IsEmailConfirmedAsync(user))
            //{
            //}

            var result = await _signInManager.PasswordSignInAsync(
                                    loginUser.Username,
                                    loginUser.Password,
                                    loginUser.RememberMe,
                                    lockoutOnFailure: true);
            if (result.Succeeded)
            {
                var (accessToken, accessTokenExpire,refreshToken,refreshTokenExpire, claims) = await _tokenStoreService.CreateJwtTokens(user, refreshTokenSource: null);

                _antiforgery.RegenerateAntiForgeryCookies(claims);

                return Ok(new { access_token = accessToken, access_token_expire_time = accessTokenExpire, refresh_token = refreshToken, refresh_token_expire_time = refreshTokenExpire });
            }

            if (result.RequiresTwoFactor)
            {
               
            }

            if (result.IsLockedOut)
            {

            }

            if (result.IsNotAllowed)
            {

            }

            return Unauthorized();
        }

        [AllowAnonymous]
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

        [HttpGet("[action]")]
        public async Task<bool> Logout(string refreshToken)
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var userIdValue = claimsIdentity.FindFirst(ClaimTypes.UserData)?.Value;

            // The Jwt implementation does not support "revoke OAuth token" (logout) by design.
            // Delete the user's tokens from the database (revoke its bearer token)
            await _tokenStoreService.RevokeUserBearerTokensAsync(userIdValue, refreshToken);
            await _uow.SaveChangesAsync();

            _antiforgery.DeleteAntiForgeryCookies();

            return true;
        }

        /// <summary>
        /// 验证密码
        /// </summary>
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
        /// 更改密码
        /// </summary>
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

                //return RedirectToAction(nameof(Index), "UserCard", routeValues: new { id = user.Id });
                return Ok();
            }

            //foreach (var error in result.Errors)
            //{
            //    ModelState.AddModelError(string.Empty, error.Description);
            //}

            return Json(result.DumpErrors(useHtmlNewLine: true));
        }

        [HttpGet("[action]"), HttpPost("[action]")]
        public bool IsAuthenticated()
        {
            return User.Identity.IsAuthenticated;
        }

        [HttpGet("[action]"), HttpPost("[action]")]
        public IActionResult GetUserInfo()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            return Json(new { Username = claimsIdentity.Name, Apps = new List<int> { 1 }, Roles = new List<string> { ConstantRoles.Admin } });
        }
    }
}