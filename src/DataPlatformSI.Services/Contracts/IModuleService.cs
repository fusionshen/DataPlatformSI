using DataPlatformSI.Entities.Modules;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataPlatformSI.Services.Contracts
{
    public interface IModuleService
    {
        Task<IdentityResult> AddModuleAsync(Module module);

        Task<IList<Module>> GetAllModulesAsync();

        Task<Module> GetModuleByIdAsync(int id);

        Task<IdentityResult> UpdateModuleAsync(Module module);

        Task<IdentityResult> DeleteModuleAsync(Module module);

        Task<Module> FindByNameAndSpaceAsyc(string name,string space);

        Task<IdentityResult> InitModuleAsync(IList<Module> modules);
    }
}
