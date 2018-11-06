using DataPlatformSI.Entities.AuditableEntity;
using Microsoft.AspNetCore.Identity;

namespace DataPlatformSI.Entities.Identity
{
    public class UserLogin : IdentityUserLogin<int>, IAuditableEntity
    {
        public virtual User User { get; set; }
    }
}