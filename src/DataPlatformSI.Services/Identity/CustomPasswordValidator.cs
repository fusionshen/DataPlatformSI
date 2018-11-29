using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataPlatformSI.Common.GuardToolkit;
using DataPlatformSI.Entities.Identity;
using DataPlatformSI.Services.Contracts.Identity;
using DataPlatformSI.ViewModels.Identity.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace DataPlatformSI.Services.Identity
{
    /// <summary>
    /// Extending the Built-in Password Validation
    /// More info: http://www.dotnettips.info/post/2579
    /// </summary>
    public class CustomPasswordValidator : PasswordValidator<User>
    {
        private readonly IUsedPasswordsService _usedPasswordsService;
        private readonly ISet<string> _passwordsBanList;

        public CustomPasswordValidator(
            IdentityErrorDescriber errors,// How to use CustomIdentityErrorDescriber
            IOptionsSnapshot<SiteSettings> configurationRoot,
            IUsedPasswordsService usedPasswordsService) : base(errors)
        {
            _usedPasswordsService = usedPasswordsService;
            _usedPasswordsService.CheckArgumentIsNull(nameof(_usedPasswordsService));

            configurationRoot.CheckArgumentIsNull(nameof(configurationRoot));
            _passwordsBanList = new HashSet<string>(configurationRoot.Value.PasswordsBanList, StringComparer.OrdinalIgnoreCase);

            if (!_passwordsBanList.Any())
            {
                throw new InvalidOperationException("Please fill the passwords ban list in the appsettings.json file.");
            }
        }

        public override async Task<IdentityResult> ValidateAsync(UserManager<User> manager, User user, string password)
        {
            var errors = new List<IdentityError>();

            if (string.IsNullOrWhiteSpace(password))
            {
                errors.Add(new IdentityError
                {
                    Code = "PasswordIsNotSet",
                    Description = "请填写密码"
                });
                return IdentityResult.Failed(errors.ToArray());
            }

            if (string.IsNullOrWhiteSpace(user?.UserName))
            {
                errors.Add(new IdentityError
                {
                    Code = "UserNameIsNotSet",
                    Description = "请填写用户名"
                });
                return IdentityResult.Failed(errors.ToArray());
            }

            // First use the built-in validator
            var result = await base.ValidateAsync(manager, user, password);
            errors = result.Succeeded ? new List<IdentityError>() : result.Errors.ToList();

            // Extending the built-in validator
            if (password.ToLower().Contains(user.UserName.ToLower()))
            {
                errors.Add(new IdentityError
                {
                    Code = "PasswordContainsUserName",
                    Description = "密码不能包含用户名的一部分"
                });
                return IdentityResult.Failed(errors.ToArray());
            }

            if (!isSafePasword(password))
            {
                errors.Add(new IdentityError
                {
                    Code = "PasswordIsNotSafe",
                    Description = "输入的密码太简单"
                });
                return IdentityResult.Failed(errors.ToArray());
            }

            if (await _usedPasswordsService.IsPreviouslyUsedPasswordAsync(user, password))
            {
                errors.Add(new IdentityError
                {
                    Code = "IsPreviouslyUsedPassword",
                    Description = "密码已被使用"
                });
                return IdentityResult.Failed(errors.ToArray());
            }

            return !errors.Any() ? IdentityResult.Success : IdentityResult.Failed(errors.ToArray());
        }

        private static bool areAllCharsEuqal(string data)
        {
            if (string.IsNullOrWhiteSpace(data)) return false;
            data = data.ToLowerInvariant();
            var firstElement = data.ElementAt(0);
            var euqalCharsLen = data.ToCharArray().Count(x => x == firstElement);
            if (euqalCharsLen == data.Length) return true;
            return false;
        }

        private bool isSafePasword(string data)
        {
            if (string.IsNullOrWhiteSpace(data)) return false;
            if (data.Length < 5) return false;
            if (_passwordsBanList.Contains(data.ToLowerInvariant())) return false;
            if (areAllCharsEuqal(data)) return false;

            return true;
        }
    }
}
