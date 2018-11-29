using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataPlatformSI.Common.GuardToolkit;
using DataPlatformSI.Entities.Identity;
using DataPlatformSI.ViewModels.Identity.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace DataPlatformSI.Services.Identity
{
    /// <summary>
    /// Extending the Built-in User Validation
    /// More info: http://www.dotnettips.info/post/2579
    /// </summary>
    public class CustomUserValidator : UserValidator<User>
    {
        private readonly ISet<string> _emailsBanList;

        public CustomUserValidator(
            IdentityErrorDescriber errors,// How to use CustomIdentityErrorDescriber
            IOptionsSnapshot<SiteSettings> configurationRoot
            ) : base(errors)
        {
            configurationRoot.CheckArgumentIsNull(nameof(configurationRoot));
            _emailsBanList = new HashSet<string>(configurationRoot.Value.EmailsBanList, StringComparer.OrdinalIgnoreCase);

            if (!_emailsBanList.Any())
            {
                throw new InvalidOperationException("Please fill the emails ban list in the appsettings.json file.");
            }
        }

        public override async Task<IdentityResult> ValidateAsync(UserManager<User> manager, User user)
        {
            // First use the built-in validator
            var result = await base.ValidateAsync(manager, user);
            var errors = result.Succeeded ? new List<IdentityError>() : result.Errors.ToList();

            // Extending the built-in validator
            ValidateEmail(user, errors);
            ValidateUserName(user, errors);

            return !errors.Any() ? IdentityResult.Success : IdentityResult.Failed(errors.ToArray());
        }

        private void ValidateEmail(User user, List<IdentityError> errors)
        {
            var userEmail = user?.Email;
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                if (string.IsNullOrWhiteSpace(userEmail))
                {
                    errors.Add(new IdentityError
                    {
                        Code = "EmailIsNotSet",
                        Description = "请填写邮箱信息."
                    });
                }
                return; // base.ValidateAsync() will cover this case
            }

            if (_emailsBanList.Any(email => userEmail.EndsWith(email, StringComparison.OrdinalIgnoreCase)))
            {
                errors.Add(new IdentityError
                {
                    Code = "BadEmailDomainError",
                    Description = "请输入有效的邮箱"
                });
            }
        }

        private static void ValidateUserName(User user, List<IdentityError> errors)
        {
            var userName = user?.UserName;
            if (string.IsNullOrWhiteSpace(userName))
            {
                if (string.IsNullOrWhiteSpace(userName))
                {
                    errors.Add(new IdentityError
                    {
                        Code = "UserIsNotSet",
                        Description = "请填写用户信息"
                    });
                }
                return;  // base.ValidateAsync() will cover this case
            }

            //if (userName.IsNumeric() || userName.ContainsNumber())
            //{
            //    errors.Add(new IdentityError
            //    {
            //        Code = "BadUserNameError",
            //        Description = "输入的用户名不能包含数字"
            //    });
            //}

            if (userName.HasConsecutiveChars())
            {
                errors.Add(new IdentityError
                {
                    Code = "BadUserNameError",
                    Description = "用户名无效"
                });
            }
        }
    }
}
