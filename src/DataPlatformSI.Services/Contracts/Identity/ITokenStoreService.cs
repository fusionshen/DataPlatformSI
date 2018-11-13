using DataPlatformSI.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DataPlatformSI.Services.Contracts.Identity
{
    public interface ITokenStoreService
    {
        Task AddUserTokenAsync(UserToken userToken);
        Task AddUserTokenAsync(User user, string refreshToken, string accessToken, string refreshTokenSource);
        Task<bool> IsValidTokenAsync(string accessToken, int userId);
        Task DeleteExpiredTokensAsync();
        Task<UserToken> FindTokenAsync(string refreshToken);
        Task DeleteTokenAsync(string refreshToken);
        Task DeleteTokensWithSameRefreshTokenSourceAsync(string refreshTokenIdHashSource);
        Task InvalidateUserTokensAsync(int userId);
        Task<(string accessToken,DateTime accessTokenExpire, string refreshToken, DateTime refreshTokenExpire, IEnumerable<Claim> Claims)> CreateJwtTokens(User user, string refreshTokenSource);
        Task RevokeUserBearerTokensAsync(string userIdValue, string refreshToken);
    }
}
