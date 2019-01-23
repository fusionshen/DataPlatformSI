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
    public class ServerService : IServerService
    {
        private readonly IUnitOfWork _uow;
        private readonly DbSet<Server> _servers;
        public ServerService(IUnitOfWork uow)
        {
            _uow = uow;
            _uow.CheckArgumentIsNull(nameof(_uow));
            _servers = _uow.Set<Server>();
        }

        public async Task<IdentityResult> AddServerAsync(Server server)
        {
            var result = await ValidateServerAsync(server);
            if (!result.Succeeded)
            {
                return result;
            }
            _servers.Add(server);
            await _uow.SaveChangesAsync();
            return IdentityResult.Success;

        }

        private async Task<IdentityResult> ValidateServerAsync(Server server)
        {
            var errors = new List<IdentityError>();

            return errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
        }

        public async Task<IList<Server>> GetAllServersAsync()
        {
            return await _servers.ToListAsync();
        }

        public async Task<Server> GetServerByIdAsync(int id)
        {
            return await _servers.FirstOrDefaultAsync(server => server.Id == id);
        }

        public async Task<IdentityResult> UpdateServerAsync(Server server)
        {
            var result = await ValidateServerAsync(server);
            if (!result.Succeeded)
            {
                return result;
            }
            _servers.Update(server);
            await _uow.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteServerAsync(Server server)
        {
            _servers.Remove(server);
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
    }
}