using DataPlatformSI.Common.GuardToolkit;
using DataPlatformSI.DataLayer.Context;
using DataPlatformSI.Entities.MDM;
using DataPlatformSI.Services.Contracts.MDM;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataPlatformSI.Services.MDM
{
    public class RepositoryService : IRepositoryService
    {
        private readonly IUnitOfWork _uow;
        private readonly DbSet<Repository> _repositories;
        public RepositoryService(IUnitOfWork uow)
        {
            _uow = uow;
            _uow.CheckArgumentIsNull(nameof(_uow));
            _repositories = _uow.Set<Repository>();
        }

        private async Task<IdentityResult> ValidateRepositoryAsync(Repository repo)
        {
            var errors = new List<IdentityError>();

            return errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
        }


        public async Task<IdentityResult> AddRepositoryAsync(Repository repo)
        {
            var result = await ValidateRepositoryAsync(repo);
            if (!result.Succeeded)
            {
                return result;
            }
            _repositories.Add(repo);
            await _uow.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteRepositoryAsync(Repository repo)
        {
            _repositories.Remove(repo);
            try
            {
                await _uow.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return IdentityResult.Failed(new IdentityErrorDescriber().ConcurrencyFailure());
            }
            return IdentityResult.Success;
        }

        public async Task<IList<Repository>> GetAllRepositoriesAsync()
        {
            return await _repositories.ToListAsync();
        }

        public async Task<Repository> GetRepositoryByIdAsync(int id)
        {
            return await _repositories.FirstOrDefaultAsync(repo => repo.Id == id);
        }

        public async Task<IdentityResult> UpdateRepositoryAsync(Repository repo)
        {
            var result = await ValidateRepositoryAsync(repo);
            if (!result.Succeeded)
            {
                return result;
            }
            _repositories.Update(repo);
            await _uow.SaveChangesAsync();
            return IdentityResult.Success;
        }
    }
}
