using DataPlatformSI.Entities.AuditableEntity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataPlatformSI.Entities.Identity
{
    public class UserClaim : IdentityUserClaim<int>, IAuditableEntity
    {
        public virtual User User { get; set; }
    }
}