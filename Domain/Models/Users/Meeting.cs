using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Users
{
    [Table("Meetings", Schema = "Meeting")]
    public class Meeting : FullAuditedEntity
    {
        public string Name { get; set; }

        public string SessionId { get; set; }

        public string Password { get; set; }

        public DateTimeOffset TimeStart { get; set; }

        public DateTimeOffset TimeEnd { get; set; }

        public string Description { get; set; }

        public short Type { get; set; }

        public bool IsNotify { get; set; }

        public Guid ChannelId { get; set; }

        public Channel Channel { get; set; }

        public short Status { get; set; } 
    }
}