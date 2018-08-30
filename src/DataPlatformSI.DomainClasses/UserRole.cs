﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DataPlatformSI.DomainClasses
{
    public class UserRole
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }

        public virtual User User { get; set; }
        public virtual Role Role { get; set; }
    }
}