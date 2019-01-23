using DataPlatformSI.Entities.MDM;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataPlatformSI.Services.Contracts.MDM
{
    public interface IRepositoryService
    {
        Task<IdentityResult> AddRepositoryAsync(Repository repo);

        Task<IList<Repository>> GetAllRepositoriesAsync();

        Task<Repository> GetRepositoryByIdAsync(int id);

        Task<IdentityResult> UpdateRepositoryAsync(Repository repo);

        Task<IdentityResult> DeleteRepositoryAsync(Repository repo);
    }
}
