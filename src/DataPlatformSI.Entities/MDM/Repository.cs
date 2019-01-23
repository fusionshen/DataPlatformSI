using DataPlatformSI.Entities.AuditableEntity;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataPlatformSI.Entities.MDM
{
    public class Repository : IAuditableEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Server { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Port { get; set; }
    }
}