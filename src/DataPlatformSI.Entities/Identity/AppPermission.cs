using System;
using System.Collections.Generic;
using System.Text;

namespace DataPlatformSI.Entities.Identity
{
    public class AppPermission
    {
        public string Permission { get; set; }

        public string Description { get; set; }

        public string Scope { get; set; }

        public string ScopeDescription { get; set; }

        public string Params { get; set; }

        public string Action { get; set; }

        public string Controller { get; set; }
    }
}