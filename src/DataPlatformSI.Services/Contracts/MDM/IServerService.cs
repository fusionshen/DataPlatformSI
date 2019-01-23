using DataPlatformSI.Entities.MDM;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataPlatformSI.Services.Contracts.MDM
{
    public interface IServerService
    {
        Task<IdentityResult> AddServerAsync(Server server);

        Task<IList<Server>> GetAllServersAsync();

        Task<Server> GetServerByIdAsync(int id);

        Task<IdentityResult> UpdateServerAsync(Server server);

        Task<IdentityResult> DeleteServerAsync(Server server);
    }
}
