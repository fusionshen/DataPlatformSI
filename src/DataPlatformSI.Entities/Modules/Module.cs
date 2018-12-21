using DataPlatformSI.Entities.AuditableEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataPlatformSI.Entities.Modules
{
    public class Module : IAuditableEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string SpaceName { get; set; }

        public string DisplayName { get; set; }

        [NotMapped]
        public string ModuleType
        {
            get
            {
                var moduleType = $"{SpaceName}.{Name},{SpaceName} , Version={Version ?? "1.0.0.0"}, Culture=neutral, PublicKeyToken=null";
                return moduleType;
            }
        }

        public string Checksum { get; set; }

        public string Description { get; set; }

        public DateTimeOffset? CreatedTime { get; set; }

        public string Version { get; set; }

        public bool? IsCore { get; set; }

        //public virtual IList<ModuleFile> Files { get; set; }
    }
}
