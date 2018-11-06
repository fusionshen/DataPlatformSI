using DataPlatformSI.Entities.AuditableEntity;
using Microsoft.AspNetCore.Identity;

namespace DataPlatformSI.Entities.Identity
{
    public class RoleClaim : IdentityRoleClaim<int>, IAuditableEntity
    {
        public virtual Role Role { get; set; }
    }
}