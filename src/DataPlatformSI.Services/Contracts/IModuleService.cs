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
    }
}
