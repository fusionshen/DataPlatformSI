using DataPlatformSI.DataLayer.Context;
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

namespace DataPlatformSI.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("CorsPolicy")]
    public class AccountController : Controller
    {
        private readonly IApplicationUserManager _userManager;
        private readonly IApplicationSignInManager _signInManager;
        private readonly ITokenStoreService _tokenStoreService;
        private readonly IUnitOfWork _uow;
        private readonly IAntiForgeryCookieService _antiforgery;

        public AccountController(
            IApplicationUserManager userManager,
            IApplicationSignInManager signInManager,
            ITokenStoreService tokenStoreService,
            IUnitOfWork uow,
            IAntiForgeryCookieService antiforgery)
        {
            _userManager = userManager;
            _userManager.CheckArgumentIsNull(nameof(userManager));

            _signInManager = signInManager;
            _signInManager.CheckArgumentIsNull(nameof(signInManager));

            _tokenStoreService = tokenStoreService;
            _tokenStoreService.CheckArgumentIsNull(nameof(tokenStoreService));

            _uow = uow;
            _uow.CheckArgumentIsNull(nameof(_uow));

            _antiforgery = antiforgery;
            _antiforgery.CheckArgumentIsNull(nameof(antiforgery));
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

            if (result.RequiresTwoFactor)
            {
                //return RedirectToAction(
                //    nameof(TwoFactorController.SendCode),
                //    "TwoFactor",
                //    new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
            }

            if (result.IsLockedOut)
            {

            }

            if (result.IsNotAllowed)
            {

            }

            //var user = await _userManager.FindUserAsync(loginUser.Username, loginUser.Password);
            //if (user == null || !user.IsActive)
            //{
            //    return Unauthorized();
            //}

            var (accessToken, refreshToken, claims) = await _tokenStoreService.CreateJwtTokens(user, refreshTokenSource: null);

            _antiforgery.RegenerateAntiForgeryCookies(claims);

            return Ok(new { access_token = accessToken, refresh_token = refreshToken });
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

            var (accessToken, newRefreshToken, claims) = await _tokenStoreService.CreateJwtTokens(token.User, refreshToken);

            _antiforgery.RegenerateAntiForgeryCookies(claims);

            return Ok(new { access_token = accessToken, refresh_token = newRefreshToken });
        }

        [AllowAnonymous]
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