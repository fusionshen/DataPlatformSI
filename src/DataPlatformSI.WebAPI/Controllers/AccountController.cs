using DataPlatformSI.DataLayer.Context;
using DataPlatformSI.DomainClasses;
using DataPlatformSI.Common;
using DataPlatformSI.Services;
using DataPlatformSI.WebAPI.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DataPlatformSI.WebAPI.Controllers
{
    [EnableCors("CorsPolicy")]
    [ODataRoutePrefix("Account")]
    public class AccountController : ODataController
    {
        private readonly IUsersService _usersService;
        private readonly ITokenStoreService _tokenStoreService;
        private readonly IUnitOfWork _uow;
        private readonly IAntiForgeryCookieService _antiforgery;

        public AccountController(
            IUsersService usersService,
            ITokenStoreService tokenStoreService,
            IUnitOfWork uow,
            IAntiForgeryCookieService antiforgery)
        {
            _usersService = usersService;
            _usersService.CheckArgumentIsNull(nameof(usersService));

            _tokenStoreService = tokenStoreService;
            _tokenStoreService.CheckArgumentIsNull(nameof(tokenStoreService));

            _uow = uow;
            _uow.CheckArgumentIsNull(nameof(_uow));

            _antiforgery = antiforgery;
            _antiforgery.CheckArgumentIsNull(nameof(antiforgery));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginUser"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        [HttpPost]
        [ODataRoute("Login")]
        public async Task<IActionResult> Login([FromBody] User loginUser)
        {
            if (loginUser == null)
            {
                return BadRequest("user is not set.");
            }

            var user = await _usersService.FindUserAsync(loginUser.Username, loginUser.Password);
            if (user == null || !user.IsActive)
            {
                return Unauthorized();
            }

            var (accessToken, refreshToken, claims) = await _tokenStoreService.CreateJwtTokens(user, refreshTokenSource: null);

            _antiforgery.RegenerateAntiForgeryCookies(claims);

            return Ok(new { access_token = accessToken, refresh_token = refreshToken });
        }

        [AllowAnonymous]
        [HttpPost]
        [ODataRoute("RefreshToken")]
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        /// http://localhost:5000/api/Account/Logout(refreshToken='232323')
        [AllowAnonymous]
        [HttpGet]
        [ODataRoute("Logout(refreshToken={refreshToken})")]
        public async Task<bool> Logout([FromODataUri]string refreshToken)
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

        [HttpGet,HttpPost]
        [ODataRoute("IsAuthenticated")]
        public bool IsAuthenticated()
        {
            return User.Identity.IsAuthenticated;
        }

        [HttpGet, HttpPost]
        [ODataRoute("GetUserInfo")]
        public IActionResult GetUserInfo()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            return Ok(new { Username = claimsIdentity.Name });
        }
    }

    public class LoginDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}