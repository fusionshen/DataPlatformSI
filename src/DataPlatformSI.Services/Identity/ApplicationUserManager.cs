﻿using DataPlatformSI.Common.GuardToolkit;
using DataPlatformSI.Common.IdentityToolkit;
using DataPlatformSI.DataLayer.Context;
using DataPlatformSI.Entities.Identity;
using DataPlatformSI.Services.Contracts.Identity;
using DataPlatformSI.ViewModels.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System;
using DNTCommon.Web.Core;
using System.Security.Claims;

namespace DataPlatformSI.Services.Identity
{
    public class ApplicationUserManager :
        UserManager<User>,
        IApplicationUserManager
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IUnitOfWork _uow;
        private readonly IUsedPasswordsService _usedPasswordsService;
        private readonly IdentityErrorDescriber _errors;
        private readonly ILookupNormalizer _keyNormalizer;
        private readonly ILogger<ApplicationUserManager> _logger;
        private readonly IOptions<IdentityOptions> _optionsAccessor;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IEnumerable<IPasswordValidator<User>> _passwordValidators;
        private readonly IServiceProvider _services;
        private readonly DbSet<User> _users;
        private readonly DbSet<Role> _roles;
        private readonly IApplicationUserStore _userStore;
        private readonly IEnumerable<IUserValidator<User>> _userValidators;
        private readonly ISecurityService _securityService;
        private User _currentUserInScope;

        public ApplicationUserManager(
            IApplicationUserStore store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<User> passwordHasher,
            IEnumerable<IUserValidator<User>> userValidators,
            IEnumerable<IPasswordValidator<User>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<ApplicationUserManager> logger,
            IHttpContextAccessor contextAccessor,
            IUnitOfWork uow,
            IUsedPasswordsService usedPasswordsService,
            ISecurityService securityService)
            : base((UserStore<User, Role, ApplicationDbContext, int, UserClaim, UserRole, UserLogin, UserToken, RoleClaim>)store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            _userStore = store;
            _userStore.CheckArgumentIsNull(nameof(_userStore));

            _optionsAccessor = optionsAccessor;
            _optionsAccessor.CheckArgumentIsNull(nameof(_optionsAccessor));

            _passwordHasher = passwordHasher;
            _passwordHasher.CheckArgumentIsNull(nameof(_passwordHasher));

            _userValidators = userValidators;
            _userValidators.CheckArgumentIsNull(nameof(_userValidators));

            _passwordValidators = passwordValidators;
            _passwordValidators.CheckArgumentIsNull(nameof(_passwordValidators));

            _keyNormalizer = keyNormalizer;
            _keyNormalizer.CheckArgumentIsNull(nameof(_keyNormalizer));

            _errors = errors;
            _errors.CheckArgumentIsNull(nameof(_errors));

            _services = services;
            _services.CheckArgumentIsNull(nameof(_services));

            _logger = logger;
            _logger.CheckArgumentIsNull(nameof(_logger));

            _contextAccessor = contextAccessor;
            _contextAccessor.CheckArgumentIsNull(nameof(_contextAccessor));

            _uow = uow;
            _uow.CheckArgumentIsNull(nameof(_uow));

            _usedPasswordsService = usedPasswordsService;
            _usedPasswordsService.CheckArgumentIsNull(nameof(_usedPasswordsService));

            _securityService = securityService;
            _securityService.CheckArgumentIsNull(nameof(_usedPasswordsService));

            _users = uow.Set<User>();
            _roles = uow.Set<Role>();
        }

        #region BaseClass

        string IApplicationUserManager.CreateTwoFactorRecoveryCode()
        {
            return base.CreateTwoFactorRecoveryCode();
        }

        Task<PasswordVerificationResult> IApplicationUserManager.VerifyPasswordAsync(IUserPasswordStore<User> store, User user, string password)
        {
            return base.VerifyPasswordAsync(store, user, password);
        }

        public override async Task<IdentityResult> CreateAsync(User user)
        {
            var result = await base.CreateAsync(user);
            if (result.Succeeded)
            {
                await _usedPasswordsService.AddToUsedPasswordsListAsync(user);
            }
            return result;
        }

        public override async Task<IdentityResult> CreateAsync(User user, string password)
        {
            var result = await base.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _usedPasswordsService.AddToUsedPasswordsListAsync(user);
            }
            return result;
        }

        public override async Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword)
        {
            var result = await base.ChangePasswordAsync(user, currentPassword, newPassword);
            if (result.Succeeded)
            {
                await _usedPasswordsService.AddToUsedPasswordsListAsync(user);
            }
            return result;
        }

        public override async Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword)
        {
            var result = await base.ResetPasswordAsync(user, token, newPassword);
            if (result.Succeeded)
            {
                await _usedPasswordsService.AddToUsedPasswordsListAsync(user);
            }
            return result;
        }

        #endregion

        #region CustomMethods

        public Task<User> FindUserAsync(int userId)
        {
            return _users.FindAsync(userId);
        }

        public Task<User> FindUserAsync(string username, string password)
        {
            var passwordHash = _securityService.GetSha256Hash(password);
            return _users.FirstOrDefaultAsync(x => x.UserName == username && x.PasswordHash == passwordHash);
        }

        public async Task<string> GetSerialNumberAsync(int userId)
        {
            var user = await FindUserAsync(userId);
            return user.SerialNumber;
        }

        public async Task UpdateUserLastActivityDateAsync(int userId)
        {
            var user = await FindUserAsync(userId);
            if (user.LastVisitDateTime != null)
            {
                var updateLastActivityDate = TimeSpan.FromMinutes(2);
                var currentUtc = DateTimeOffset.UtcNow;
                var timeElapsed = currentUtc.Subtract(user.LastVisitDateTime.Value);
                if (timeElapsed < updateLastActivityDate)
                {
                    return;
                }
            }
            user.LastVisitDateTime = DateTimeOffset.UtcNow;
            await _uow.SaveChangesAsync();
        }

        //public int GetCurrentUserId()
        //{
        //    var claimsIdentity = _contextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        //    var userDataClaim = claimsIdentity?.FindFirst(ClaimTypes.UserData);
        //    var userId = userDataClaim?.Value;
        //    return string.IsNullOrWhiteSpace(userId) ? 0 : int.Parse(userId);
        //}

        //public Task<User> GetCurrentUserAsync()
        //{
        //    var userId = GetCurrentUserId();
        //    return FindUserAsync(userId);
        //}

        //public async Task<(bool Succeeded, string Error)> ChangePasswordAsync(User user, string currentPassword, string newPassword)
        //{
        //    var currentPasswordHash = _securityService.GetSha256Hash(currentPassword);
        //    if (user.Password != currentPasswordHash)
        //    {
        //        return (false, "Current password is wrong.");
        //    }

        //    user.Password = _securityService.GetSha256Hash(newPassword);
        //    // user.SerialNumber = Guid.NewGuid().ToString("N"); // To force other logins to expire.
        //    await _uow.SaveChangesAsync();
        //    return (true, string.Empty);
        //}


        public User FindById(int userId)
        {
            return _users.Find(userId);
        }

        public Task<User> FindByIdIncludeUserRolesAsync(int userId)
        {
            return _users.Include(x => x.Roles).FirstOrDefaultAsync(x => x.Id == userId);
        }

        public Task<List<User>> GetAllUsersAsync()
        {
            return Users.ToListAsync();
        }

        public User GetCurrentUser()
        {
            if (_currentUserInScope != null)
            {
                return _currentUserInScope;
            }

            var currentUserId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return null;
            }

            var userId = int.Parse(currentUserId);
            return _currentUserInScope = FindById(userId);
        }

        public async Task<User> GetCurrentUserAsync()
        {
            return _currentUserInScope ??
                (_currentUserInScope = await GetUserAsync(_contextAccessor.HttpContext.User));
        }

        public string GetCurrentUserId()
        {
            return _contextAccessor.HttpContext.User.Identity.GetUserId();
        }

        public int? CurrentUserId
        {
            get
            {
                if (string.IsNullOrEmpty(_contextAccessor.HttpContext.User.Identity.GetUserId()))
                {
                    return null;
                }

                return !int.TryParse(_contextAccessor.HttpContext.User.Identity.GetUserId(), out int result) ? (int?)null : result;
            }
        }

        IPasswordHasher<User> IApplicationUserManager.PasswordHasher { get => base.PasswordHasher; set => base.PasswordHasher = value; }

        IList<IUserValidator<User>> IApplicationUserManager.UserValidators => base.UserValidators;

        IList<IPasswordValidator<User>> IApplicationUserManager.PasswordValidators => base.PasswordValidators;

        IQueryable<User> IApplicationUserManager.Users => base.Users;

        public string GetCurrentUserName()
        {
            return _contextAccessor.HttpContext.User.Identity.GetUserName();
        }

        public async Task<bool> HasPasswordAsync(int userId)
        {
            var user = await FindByIdAsync(userId.ToString());
            return user?.PasswordHash != null;
        }

        public async Task<bool> HasPhoneNumberAsync(int userId)
        {
            var user = await FindByIdAsync(userId.ToString());
            return user?.PhoneNumber != null;
        }

        public async Task<byte[]> GetEmailImageAsync(int? userId)
        {
            if (userId == null)
                return "?".TextToImage(new TextToImageOptions());

            var user = await FindByIdAsync(userId.Value.ToString());
            if (user == null)
                return "?".TextToImage(new TextToImageOptions());

            if (!user.IsEmailPublic)
                return "?".TextToImage(new TextToImageOptions());

            return user.Email.TextToImage(new TextToImageOptions());
        }

        public async Task<PagedUsersListViewModel> GetPagedUsersListAsync(SearchUsersViewModel model, int pageNumber)
        {
            var skipRecords = pageNumber * model.MaxNumberOfRows;
            var query = _users.Include(x => x.Roles).AsNoTracking();

            if (!model.ShowAllUsers)
            {
                query = query.Where(x => x.IsActive == model.UserIsActive);
            }

            if (!string.IsNullOrWhiteSpace(model.TextToFind))
            {
                //model.TextToFind = model.TextToFind.ApplyCorrectYeKe();

                if (model.IsPartOfEmail)
                {
                    query = query.Where(x => x.Email.Contains(model.TextToFind));
                }

                if (model.IsUserId)
                {
                    if (int.TryParse(model.TextToFind, out int userId))
                    {
                        query = query.Where(x => x.Id == userId);
                    }
                }

                if (model.IsPartOfName)
                {
                    query = query.Where(x => x.FirstName.Contains(model.TextToFind));
                }

                if (model.IsPartOfLastName)
                {
                    query = query.Where(x => x.LastName.Contains(model.TextToFind));
                }

                if (model.IsPartOfUserName)
                {
                    query = query.Where(x => x.UserName.Contains(model.TextToFind));
                }

                if (model.IsPartOfLocation)
                {
                    query = query.Where(x => x.Location.Contains(model.TextToFind));
                }
            }

            if (model.HasEmailConfirmed)
            {
                query = query.Where(x => x.EmailConfirmed);
            }

            if (model.UserIsLockedOut)
            {
                query = query.Where(x => x.LockoutEnd != null);
            }

            if (model.HasTwoFactorEnabled)
            {
                query = query.Where(x => x.TwoFactorEnabled);
            }

            query = query.OrderBy(x => x.Id);
            return new PagedUsersListViewModel
            {
                Paging =
                {
                    TotalItems = await query.CountAsync()
                },
                Users = await query.Skip(skipRecords).Take(model.MaxNumberOfRows).ToListAsync(),
                Roles = await _roles.ToListAsync()
            };
        }

        public async Task<PagedUsersListViewModel> GetPagedUsersListAsync(
            int pageNumber, int recordsPerPage,
            string sortByField, SortOrder sortOrder,
            bool showAllUsers)
        {
            var skipRecords = pageNumber * recordsPerPage;
            var query = _users.Include(x => x.Roles).AsNoTracking();

            if (!showAllUsers)
            {
                query = query.Where(x => x.IsActive);
            }

            switch (sortByField)
            {
                case nameof(User.Id):
                    query = sortOrder == SortOrder.Descending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id);
                    break;
                default:
                    query = sortOrder == SortOrder.Descending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id);
                    break;
            }

            return new PagedUsersListViewModel
            {
                Paging =
                {
                    TotalItems = await query.CountAsync()
                },
                Users = await query.Skip(skipRecords).Take(recordsPerPage).ToListAsync(),
                Roles = await _roles.ToListAsync()
            };
        }

        public async Task<IdentityResult> UpdateUserAndSecurityStampAsync(int userId, Action<User> action)
        {
            var user = await FindByIdIncludeUserRolesAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "UserNotFound",
                    Description = "کاربر مورد نظر یافت نشد."
                });
            }

            action(user);

            var result = await UpdateAsync(user);
            if (!result.Succeeded)
            {
                return result;
            }
            return await UpdateSecurityStampAsync(user);
        }

        public async Task<IdentityResult> AddOrUpdateUserRolesAsync(int userId, IList<int> selectedRoleIds, Action<User> action = null)
        {
            var user = await FindByIdIncludeUserRolesAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "UserNotFound",
                    Description = "کاربر مورد نظر یافت نشد."
                });
            }

            var currentUserRoleIds = user.Roles.Select(x => x.RoleId).ToList();

            if (selectedRoleIds == null)
            {
                selectedRoleIds = new List<int>();
            }
            var newRolesToAdd = selectedRoleIds.Except(currentUserRoleIds).ToList();
            foreach (var roleId in newRolesToAdd)
            {
                user.Roles.Add(new UserRole { RoleId = roleId, UserId = user.Id });
            }

            var removedRoles = currentUserRoleIds.Except(selectedRoleIds).ToList();
            foreach (var roleId in removedRoles)
            {
                var userRole = user.Roles.SingleOrDefault(ur => ur.RoleId == roleId);
                if (userRole != null)
                {
                    user.Roles.Remove(userRole);
                }
            }

            action?.Invoke(user);

            var result = await UpdateAsync(user);
            if (!result.Succeeded)
            {
                return result;
            }
            return await UpdateSecurityStampAsync(user);
        }

        #endregion
    }
}
