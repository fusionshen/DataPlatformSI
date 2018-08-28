using DataPlatformSI.WebAPI.Models;
using DataPlatformSI.WebAPI.Models.DTOs;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DataPlatformSI.WebAPI.Controllers
{
    //[ApiVersion("1.0")]
    //[ApiVersion("0.9", Deprecated = true)]
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
                return Ok (GenerateJwtToken(model.UserName, user));
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

        //public class LoginDto
        //{
        //    [Required]
        //    public string UserName { get; set; }

        //    [Required]
        //    public string Password { get; set; }

        //    public bool RememberMe { get; set; }

        //}

        //public class RegisterDto
        //{
        //    [Required]
        //    public string UserName { get; set; }

        //    [Required]
        //    [StringLength(100, ErrorMessage = "PASSWORD_MIN_LENGTH", MinimumLength = 6)]
        //    public string Password { get; set; }
        //}
    }
}