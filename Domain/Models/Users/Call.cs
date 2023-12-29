using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Users
{
    public class Call : FullAuditedEntity
    {
        public string Name { get; set; }

        public string SessionId { get; set; }

        public string Password { get; set; }

        public short Status { get; set; }
    }
}