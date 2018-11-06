using DataPlatformSI.Entities.Identity;
using DataPlatformSI.ViewModels.Identity;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DataPlatformSI.Services.Contracts.Identity
{
    public interface ISiteStatService
    {
        Task<List<User>> GetOnlineUsersListAsync(int numbersToTake, int minutesToTake);

        Task<List<User>> GetTodayBirthdayListAsync();

        Task UpdateUserLastVisitDateTimeAsync(ClaimsPrincipal claimsPrincipal);

        Task<AgeStatViewModel> GetUsersAverageAge();
    }
}