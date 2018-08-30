using DataPlatformSI.DataLayer.Context;
using DataPlatformSI.DomainClasses;
using DataPlatformSI.Common;
using DataPlatformSI.Services;
using DataPlatformSI.WebAPI.Models;
using DataPlatformSI.WebAPI.Models.DTOs;
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
    //[Route("api/[controller]")]
    //[EnableCors("CorsPolicy")]
    //[ApiVersionNeutral]
    //[ODataRoutePrefix("Account")]
    //public class AccountController : ODataController
    //{
    //    private readonly IUsersService _usersService;
    //    private readonly ITokenStoreService _tokenStoreService;
    //    private readonly IUnitOfWork _uow;
    //    private readonly IAntiForgeryCookieService _antiforgery;

    //    public AccountController(
    //        IUsersService usersService,
    //        ITokenStoreService tokenStoreService,
    //        IUnitOfWork uow,
    //        IAntiForgeryCookieService antiforgery)
    //    {
    //        _usersService = usersService;
    //        _usersService.CheckArgumentIsNull(nameof(usersService));

    //        _tokenStoreService = tokenStoreService;
    //        _tokenStoreService.CheckArgumentIsNull(nameof(tokenStoreService));

    //        _uow = uow;
    //        _uow.CheckArgumentIsNull(nameof(_uow));

    //        _antiforgery = antiforgery;
    //        _antiforgery.CheckArgumentIsNull(nameof(antiforgery));
    //    }

    //    [AllowAnonymous]
    //    [IgnoreAntiforgeryToken]
    //    [HttpPost("[action]")]
    //    public async Task<IActionResult> Login([FromBody]  User loginUser)
    //    {
    //        if (loginUser == null)
    //        {
    //            return BadRequest("user is not set.");
    //        }

    //        var user = await _usersService.FindUserAsync(loginUser.Username, loginUser.Password);
    //        if (user == null || !user.IsActive)
    //        {
    //            return Unauthorized();
    //        }

    //        var (accessToken, refreshToken, claims) = await _tokenStoreService.CreateJwtTokens(user, refreshTokenSource: null);

    //        _antiforgery.RegenerateAntiForgeryCookies(claims);

    //        return Ok(new { access_token = accessToken, refresh_token = refreshToken });
    //    }

    //    [AllowAnonymous]
    //    [HttpPost("[action]")]
    //    public async Task<IActionResult> RefreshToken([FromBody]JToken jsonBody)
    //    {
    //        var refreshToken = jsonBody.Value<string>("refreshToken");
    //        if (string.IsNullOrWhiteSpace(refreshToken))
    //        {
    //            return BadRequest("refreshToken is not set.");
    //        }

    //        var token = await _tokenStoreService.FindTokenAsync(refreshToken);
    //        if (token == null)
    //        {
    //            return Unauthorized();
    //        }

    //        var (accessToken, newRefreshToken, claims) = await _tokenStoreService.CreateJwtTokens(token.User, refreshToken);

    //        _antiforgery.RegenerateAntiForgeryCookies(claims);

    //        return Ok(new { access_token = accessToken, refresh_token = newRefreshToken });
    //    }

    //    [AllowAnonymous]
    //    [HttpGet("[action]")]
    //    public async Task<bool> Logout(string refreshToken)
    //    {
    //        var claimsIdentity = this.User.Identity as ClaimsIdentity;
    //        var userIdValue = claimsIdentity.FindFirst(ClaimTypes.UserData)?.Value;

    //        // The Jwt implementation does not support "revoke OAuth token" (logout) by design.
    //        // Delete the user's tokens from the database (revoke its bearer token)
    //        await _tokenStoreService.RevokeUserBearerTokensAsync(userIdValue, refreshToken);
    //        await _uow.SaveChangesAsync();

    //        _antiforgery.DeleteAntiForgeryCookies();

    //        return true;
    //    }

    //    [HttpGet("[action]"), HttpPost("[action]")]
    //    public bool IsAuthenticated()
    //    {
    //        return User.Identity.IsAuthenticated;
    //    }

    //    [HttpGet("[action]"), HttpPost("[action]")]
    //    public IActionResult GetUserInfo()
    //    {
    //        var claimsIdentity = User.Identity as ClaimsIdentity;
    //        return Ok(new { Username = claimsIdentity.Name });
    //    }
    //}

    [ApiVersionNeutral]
    [ODataRoutePrefix("Account")]
    public class AccountController : ODataController
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpPost]
        [ODataRoute("Login")]
        public async Task<object> Login([FromBody] LoginDto model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);

            if (result.Succeeded)
            {
                var appUser = _userManager.Users.SingleOrDefault(r => r.UserName == model.UserName);
                return Ok(GenerateJwtToken(model.UserName, appUser));
            }

            throw new ApplicationException("INVALID_LOGIN_ATTEMPT");
        }

        [HttpPost]
        [ODataRoute("Register")]
        public async Task<object> Register([FromBody] RegisterDto model)
        {
            var user = new ApplicationUser
            {
                UserName = model.UserName
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);
                return Ok(GenerateJwtToken(model.UserName, user));
            }

            throw new ApplicationException("UNKNOWN_ERROR");
        }

        [Authorize]
        [HttpGet]
        [ODataRoute("Protected")]
        public async Task<object> Protected()
        {
            return "Protected area";
        }

        private string GenerateJwtToken(string email, IdentityUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["JwtExpireDays"]));

            var token = new JwtSecurityToken(
                _configuration["JwtIssuer"],
                _configuration["JwtIssuer"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}