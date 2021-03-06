﻿using DataPlatformSI.Common.GuardToolkit;
using DataPlatformSI.DataLayer.Context;
using DataPlatformSI.Entities.Identity;
using DataPlatformSI.Services.Contracts.Identity;
using DataPlatformSI.ViewModels.Identity.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DataPlatformSI.Services.Identity
{
    public class TokenStoreService : ITokenStoreService
    {
        private readonly ISecurityService _securityService;
        private readonly IUnitOfWork _uow;
        private readonly DbSet<UserToken> _tokens;
        private readonly IOptionsSnapshot<SiteSettings> _configuration;
        private readonly IApplicationRoleManager _rolesService;

        public TokenStoreService(
            IUnitOfWork uow,
            ISecurityService securityService,
            IApplicationRoleManager rolesService,
            IOptionsSnapshot<SiteSettings> configuration)
        {
            _uow = uow;
            _uow.CheckArgumentIsNull(nameof(_uow));

            _securityService = securityService;
            _securityService.CheckArgumentIsNull(nameof(_securityService));

            _rolesService = rolesService;
            _rolesService.CheckArgumentIsNull(nameof(rolesService));

            _tokens = _uow.Set<UserToken>();

            _configuration = configuration;
            _configuration.CheckArgumentIsNull(nameof(configuration));
        }

        public async Task AddUserTokenAsync(UserToken userToken)
        {
            if (!_configuration.Value.BearerTokens.AllowMultipleLoginsFromTheSameUser)
            {
                await InvalidateUserTokensAsync(userToken.UserId);
            }
            await DeleteTokensWithSameRefreshTokenSourceAsync(userToken.RefreshTokenIdHashSource);
            _tokens.Add(userToken);
        }

        public async Task AddUserTokenAsync(User user, string refreshToken, string accessToken, string refreshTokenSource)
        {
            var now = DateTimeOffset.UtcNow;
            var token = new UserToken
            {
                UserId = user.Id,
                // Refresh token handles should be treated as secrets and should be stored hashed
                RefreshTokenIdHash = _securityService.GetSha256Hash(refreshToken),
                RefreshTokenIdHashSource = string.IsNullOrWhiteSpace(refreshTokenSource) ?
                                           null : _securityService.GetSha256Hash(refreshTokenSource),
                AccessTokenHash = _securityService.GetSha256Hash(accessToken),
                RefreshTokenExpiresDateTime = now.AddMinutes(_configuration.Value.BearerTokens.RefreshTokenExpirationMinutes),
                AccessTokenExpiresDateTime = now.AddMinutes(_configuration.Value.BearerTokens.AccessTokenExpirationMinutes),
                LoginProvider = "wpf client",
                Name = "some words"
            };
            await AddUserTokenAsync(token);
        }

        public async Task DeleteExpiredTokensAsync()
        {
            var now = DateTimeOffset.UtcNow;
            await _tokens.Where(x => x.RefreshTokenExpiresDateTime < now)
                         .ForEachAsync(userToken =>
                         {
                             _tokens.Remove(userToken);
                         });
        }

        public async Task DeleteTokenAsync(string refreshToken)
        {
            var token = await FindTokenAsync(refreshToken);
            if (token != null)
            {
                _tokens.Remove(token);
            }
        }

        public async Task DeleteTokensWithSameRefreshTokenSourceAsync(string refreshTokenIdHashSource)
        {
            if (string.IsNullOrWhiteSpace(refreshTokenIdHashSource))
            {
                return;
            }
            await _tokens.Where(t => t.RefreshTokenIdHashSource == refreshTokenIdHashSource)
                         .ForEachAsync(userToken =>
                         {
                             _tokens.Remove(userToken);
                         });
        }

        public async Task RevokeUserBearerTokensAsync(string userIdValue, string refreshToken)
        {
            if (!string.IsNullOrWhiteSpace(userIdValue) && int.TryParse(userIdValue, out int userId))
            {
                if (_configuration.Value.BearerTokens.AllowSignoutAllUserActiveClients)
                {
                    await InvalidateUserTokensAsync(userId);
                }
            }

            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                var refreshTokenIdHashSource = _securityService.GetSha256Hash(refreshToken);
                await DeleteTokensWithSameRefreshTokenSourceAsync(refreshTokenIdHashSource);
            }

            await DeleteExpiredTokensAsync();
        }

        public Task<UserToken> FindTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return null;
            }
            var refreshTokenIdHash = _securityService.GetSha256Hash(refreshToken);
            return _tokens.Include(x => x.User).FirstOrDefaultAsync(x => x.RefreshTokenIdHash == refreshTokenIdHash);
        }

        public async Task InvalidateUserTokensAsync(int userId)
        {
            await _tokens.Where(x => x.UserId == userId)
                         .ForEachAsync(userToken =>
                         {
                             _tokens.Remove(userToken);
                         });
        }

        public async Task<bool> IsValidTokenAsync(string accessToken, int userId)
        {
            var accessTokenHash = _securityService.GetSha256Hash(accessToken);
            var userToken = await _tokens.FirstOrDefaultAsync(
                x => x.AccessTokenHash == accessTokenHash && x.UserId == userId);
            return userToken?.AccessTokenExpiresDateTime >= DateTimeOffset.UtcNow;
        }

        public async Task<(string accessToken,DateTime accessTokenExpire, string refreshToken,DateTime refreshTokenExpire, IEnumerable<Claim> Claims)> CreateJwtTokens(User user, string refreshTokenSource)
        {
            var (AccessToken, Claims) = await CreateAccessTokenAsync(user);
            var refreshToken = Guid.NewGuid().ToString().Replace("-", "");
            await AddUserTokenAsync(user, refreshToken, AccessToken, refreshTokenSource);
            await _uow.SaveChangesAsync();

            return (AccessToken,DateTime.UtcNow.AddMinutes(_configuration.Value.BearerTokens.AccessTokenExpirationMinutes), refreshToken, DateTime.UtcNow.AddMinutes(_configuration.Value.BearerTokens.RefreshTokenExpirationMinutes), Claims);
        }

        private async Task<(string AccessToken, IEnumerable<Claim> Claims)> CreateAccessTokenAsync(User user)
        {
            var claims = new List<Claim>
            {
                // Unique Id for all Jwt tokes
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString(), ClaimValueTypes.String, _configuration.Value.BearerTokens.Issuer),
                // Issuer
                new Claim(JwtRegisteredClaimNames.Iss, _configuration.Value.BearerTokens.Issuer, ClaimValueTypes.String, _configuration.Value.BearerTokens.Issuer),
                // Issued at
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64, _configuration.Value.BearerTokens.Issuer),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(), ClaimValueTypes.String, _configuration.Value.BearerTokens.Issuer),
                new Claim(ClaimTypes.Name, user.UserName, ClaimValueTypes.String, _configuration.Value.BearerTokens.Issuer),
                new Claim("DisplayName", user.DisplayName, ClaimValueTypes.String, _configuration.Value.BearerTokens.Issuer),
                // to invalidate the cookie
                new Claim(ClaimTypes.SerialNumber, user.SecurityStamp, ClaimValueTypes.String, _configuration.Value.BearerTokens.Issuer),
                // custom data
                new Claim(ClaimTypes.UserData, user.Id.ToString(), ClaimValueTypes.String, _configuration.Value.BearerTokens.Issuer)
            };

            // add roles
            var roles = await _rolesService.FindUserRolesAsync(user.Id);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name, ClaimValueTypes.String, _configuration.Value.BearerTokens.Issuer));
                //add permissions
                foreach (var permission in role.Claims)
                {
                    claims.Add(new Claim(ConstantPolicies.DynamicPermissionClaimType, permission.ClaimValue, ClaimValueTypes.String, _configuration.Value.BearerTokens.Issuer));
                }
            }
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.Value.BearerTokens.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var now = DateTime.UtcNow;
            var token = new JwtSecurityToken(
                issuer: _configuration.Value.BearerTokens.Issuer,
                audience: _configuration.Value.BearerTokens.Audience,
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(_configuration.Value.BearerTokens.AccessTokenExpirationMinutes),
                signingCredentials: creds);
            return (new JwtSecurityTokenHandler().WriteToken(token), claims);
        }
    }
}