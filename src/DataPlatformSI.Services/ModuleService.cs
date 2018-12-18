using DataPlatformSI.Common.GuardToolkit;
using DataPlatformSI.DataLayer.Context;
using DataPlatformSI.Entities.Modules;
using DataPlatformSI.Services.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataPlatformSI.Services
{
    public class ModuleService : IModuleService
    {
        IUnitOfWork _uow;
        readonly DbSet<Module> _modules;
        public ModuleService(IUnitOfWork uow)
        {
            _uow = uow;
            _uow.CheckArgumentIsNull(nameof(_uow));

            _modules = _uow.Set<Module>();
        }

        public async Task<IdentityResult> AddModuleAsync(Module module)
        {
            var result = await ValidateModuleAsync(module);
            if (!result.Succeeded)
            {
                return result;
            }
            _modules.Add(module);
            return IdentityResult.Success;

        }

        private async Task<IdentityResult> ValidateModuleAsync(Module module)
        {
            return IdentityResult.Success;
        }

        public async Task<IList<Module>> GetAllModulesAsync()
        {
            return await _modules.ToListAsync();
        }
    }
}