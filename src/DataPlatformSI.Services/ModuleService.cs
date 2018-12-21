using DataPlatformSI.Common.GuardToolkit;
using DataPlatformSI.DataLayer.Context;
using DataPlatformSI.Entities.Modules;
using DataPlatformSI.Services.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
            await _uow.SaveChangesAsync();
            return IdentityResult.Success;

        }

        private async Task<IdentityResult> ValidateModuleAsync(Module module)
        {
            var errors = new List<IdentityError>();

            return errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
        }

        public async Task<IList<Module>> GetAllModulesAsync()
        {
            return await _modules.ToListAsync();
        }

        public Task<Module> GetModuleByIdAsync(int id)
        {
            return _modules.FirstOrDefaultAsync(module => module.Id == id);
        }

        public async Task<IdentityResult> UpdateModuleAsync(Module module)
        {
            var result = await ValidateModuleAsync(module);
            if (!result.Succeeded)
            {
                return result;
            }
            _modules.Update(module);
            await _uow.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteModuleAsync(Module module)
        {
            _modules.Remove(module);
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

        public Task<Module> FindByNameAndSpaceAsyc(string name, string space)
        {
            return _modules.FirstOrDefaultAsync(module => module.Name == name && module.SpaceName == space);
        }

        public async Task<IdentityResult> InitModuleAsync(IList<Module> experts)
        {
            var expertValues = experts.Select(module => $"{module.SpaceName}.{module.Name}").ToList() ?? new List<string>();
            var currentValues = _modules.Where(module => module.IsCore == false).Select(module => $"{module.SpaceName}.{module.Name}").ToList() ?? new List<string>();
            var newValuesToAdd = expertValues.Except(currentValues).ToList();
            foreach (var item in newValuesToAdd)
            {
                var add = experts.SingleOrDefault(module => $"{module.SpaceName}.{module.Name}" == item);
                if (add != null)
                {
                    _modules.Add(new Module
                    {
                        Name = add.Name,
                        SpaceName = add.SpaceName,
                        DisplayName = add.DisplayName,
                        Checksum = Guid.NewGuid().ToString("N"),
                        CreatedTime = DateTimeOffset.UtcNow,
                        Version = "1.0.0.0",
                        IsCore = false
                    });
                }
            }

            var removeValues = currentValues.Except(expertValues).ToList();
            foreach (var item in removeValues)
            {
                var remove = _modules.SingleOrDefault(module => $"{module.SpaceName}.{module.Name}" == item);
                if (remove != null)
                {
                    _modules.Remove(remove);
                }
            }

            //Intersect
            var updateValue = currentValues.Intersect(expertValues).ToList();
            foreach (var item in updateValue)
            {
                var to = _modules.SingleOrDefault(module => $"{module.SpaceName}.{module.Name}" == item);
                var from = experts.SingleOrDefault(module => $"{module.SpaceName}.{module.Name}" == item);
                if (to != null && from != null)
                {
                    to.Name = from.Name;
                    to.SpaceName = from.SpaceName;
                    to.DisplayName = from.DisplayName;
                    to.Description = string.Empty;
                    to.IsCore = false;
                    _modules.Update(to);
                }
            }
            await _uow.SaveChangesAsync();
            return IdentityResult.Success;
        }
    }
}