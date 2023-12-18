using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Users
{
    public class FileDomain : FullAuditedEntity
    {
        public string Url { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public Guid MessageId { get; set; } 

        public Message Message { get; set; }                
    }
}