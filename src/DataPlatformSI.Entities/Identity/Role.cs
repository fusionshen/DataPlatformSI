using DataPlatformSI.Entities.AuditableEntity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataPlatformSI.Entities.Identity
{
    public class Role : IdentityRole<int>, IAuditableEntity
    {
        public Role()
        {
        }

        public Role(string name)
            : this()
        {
            Name = name;
        }

        public Role(string name,string displayName) 
            : this(name)
        {
            DisplayName = displayName;
        }

        public Role(string name, string displayName, string description)
            : this(name,displayName)
        {
            Description = description;
        }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public virtual ICollection<UserRole> Users { get; set; }

        public virtual ICollection<RoleClaim> Claims { get; set; }
    }
}