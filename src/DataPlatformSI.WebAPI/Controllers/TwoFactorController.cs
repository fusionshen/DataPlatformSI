using DataPlatformSI.Services.Contracts.Identity;
using DataPlatformSI.Common.GuardToolkit;
using DataPlatformSI.ViewModels.Identity.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataPlatformSI.ViewModels.Identity.Emails;
using DataPlatformSI.ViewModels.Identity;
using Microsoft.AspNetCore.Cors;

namespace DataPlatformSI.WebAPI.Controllers
{
    /// <summary>
    /// 两步登录
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [EnableCors("CorsPolicy")]
    public class TwoFactorController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger<TwoFactorController> _logger;
        private readonly IApplicationSignInManager _signInManager;
        private readonly IApplicationUserManager _userManager;
        private readonly IOptionsSnapshot<SiteSettings> _siteOptions;
        private readonly ITokenStoreService _tokenStoreService;
        private readonly IAntiForgeryCookieService _antiforgery;

        public TwoFactorController(
            IApplicationUserManager userManager,
            IApplicationSignInManager signInManager,
            IEmailSender emailSender,
            IOptionsSnapshot<SiteSettings> siteOptions,
            ILogger<TwoFactorController> logger,
            ITokenStoreService tokenStoreService,
            IAntiForgeryCookieService antiforgery)
        {
            _userManager = userManager;
            _userManager.CheckArgumentIsNull(nameof(_userManager));

            _signInManager = signInManager;
            _signInManager.CheckArgumentIsNull(nameof(_signInManager));

            _emailSender = emailSender;
            _emailSender.CheckArgumentIsNull(nameof(_emailSender));

            _siteOptions = siteOptions;
            _siteOptions.CheckArgumentIsNull(nameof(_siteOptions));

            _logger = logger;
            _logger.CheckArgumentIsNull(nameof(_logger));

            _tokenStoreService = tokenStoreService;
            _tokenStoreService.CheckArgumentIsNull(nameof(tokenStoreService));

            _antiforgery = antiforgery;
            _antiforgery.CheckArgumentIsNull(nameof(antiforgery));
        }

        /// <summary>
        /// 发送两步登录验证码
        /// </summary>
        /// <param name="returnUrl">返回路径</param>
        /// <param name="rememberMe">记住登录</param>
        /// <returns>期望返回</returns>
        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> SendCode(string returnUrl = null, bool rememberMe = false)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return NotFound();
            }

            var tokenProvider = "Email";
            var code = await _userManager.GenerateTwoFactorTokenAsync(user, tokenProvider);
            if (string.IsNullOrWhiteSpace(code))
            {
                return Json("ops!error!");
            }

            await _emailSender.SendEmailAsync(
                               email: user.Email,
                               subject: "两步登录验证码",
                               viewNameOrPath: "~/Areas/Identity/Views/EmailTemplates/_TwoFactorSendCode.cshtml",
                               model: new TwoFactorSendCodeViewModel
                               {
                                   Token = code,
                                   EmailSignature = _siteOptions.Value.Smtp.FromName,
                                   MessageDateTime = DateTime.UtcNow.ToLocalTime().ToString()
                               });

            //return RedirectToAction(
            //    nameof(VerifyCode),
            //    new
            //    {
            //        Provider = tokenProvider,
            //        ReturnUrl = returnUrl,
            //        RememberMe = rememberMe
            //    });
            return Ok(new VerifyCodeViewModel { Provider = tokenProvider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        /// <summary>
        /// 两步登录校验
        /// </summary>
        /// <param name="model">期望输入</param>
        /// <returns>期望输出</returns>
        [HttpPost("[action]")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            // The following code protects for brute force attacks against the two factor codes.
            // If a user enters incorrect codes for a specified amount of time then the user account
            // will be locked out for a specified amount of time.
            var result = await _signInManager.TwoFactorSignInAsync(
                model.Provider,
                model.Code,
                model.RememberMe,
                model.RememberBrowser);

            if (result.Succeeded)
            {
                var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
                if (user == null)
                {
                    return NotFound();
                }

                var (accessToken, accessTokenExpire, refreshToken, refreshTokenExpire, claims) = await _tokenStoreService.CreateJwtTokens(user, refreshTokenSource: null);

                _antiforgery.RegenerateAntiForgeryCookies(claims);

                return Ok(new { access_token = accessToken, access_token_expire_time = accessTokenExpire, refresh_token = refreshToken, refresh_token_expire_time = refreshTokenExpire });
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning(7, "User account locked out.");
                return Json("Lockout");
            }
            return BadRequest("2factor error");
        }
    }
}