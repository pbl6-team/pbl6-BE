using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Users
{
    [Table("FileOfMessages", Schema = "Chat")]
    public class FileOfMessage : FullAuditedEntity
    {
        public string Url { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public Guid? MessageId { get; set; } 

        public Message Message { get; set; }                
    }
}