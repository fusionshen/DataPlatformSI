using System;
using System.Collections.Generic;
using System.Text;

namespace DataPlatformSI.DomainClasses
{
    public class MetadataItem
    {
        public int Id { get; set; }

        public string IconName { get; set; }

        public string Name { get; set; }

        public string AliasName { get; set; }

        public IList<string> Authors { get; set; }

        public IList<string> Keywords { get; set; }

        public string Description { get; set; }

        public double Rate { get; set; }

        public long DownloadCount { get; set; }

        public DateTime ReleaseTime { get; set; }

        public DateTime CreatedTime { get; set; }

        public string Version { get; set; }

        public string Checksum { get; set; }

    }
}
