using DataPlatformSI.Entities.AuditableEntity;
using System;
using System.Collections.Generic;

namespace DataPlatformSI.Entities.Modules
{
    public class Module : IAuditableEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string SpaceName { get; set; }

        public string DisplayName { get; set; }

        public string ModuleType { get; set; }

        public string Checksum { get; set; }

        public string Description { get; set; }

        public DateTimeOffset? CreatedTime { get; set; }

        public string Version { get; set; }

        public bool IsCustom { get; set; }

        //public virtual IList<ModuleFile> Files { get; set; }
    }
}
