using DataPlatformSI.Entities.AuditableEntity;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataPlatformSI.Entities
{
    public class Category : IAuditableEntity
    {
        public int Id { get; set; }

        public Category()
        {
            Products = new HashSet<Product>();
        }

        public string Name { get; set; }

        public string Title { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
