using DataPlatformSI.Entities.MDM;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataPlatformSI.Services.Contracts.MDM
{
    public interface IMDMServerService
    {
        Task<IdentityResult> AddServerAsync(MDMServer server);

        Task<IList<MDMServer>> GetAllServersAsync();

        Task<MDMServer> GetServerByIdAsync(int id);

        Task<IdentityResult> UpdateServerAsync(MDMServer server);

        Task<IdentityResult> DeleteServerAsync(MDMServer server);
    }
}
