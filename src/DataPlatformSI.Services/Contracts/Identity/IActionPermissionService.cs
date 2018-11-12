using DataPlatformSI.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DataPlatformSI.Services.Contracts.Identity
{
    public interface IActionPermissionService
    {
        IEnumerable<AppPermission> GetAllActionByAssembly(string assemblyName);
    }
}
