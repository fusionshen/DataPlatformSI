using DataPlatformSI.Entities.AuditableEntity;

namespace DataPlatformSI.Entities.MDM
{
    public class Server : IAuditableEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}