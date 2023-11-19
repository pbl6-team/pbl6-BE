using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Users
{
    [Table("ChannelMembers", Schema = "Chat")]
    public class ChannelMember : FullAuditedEntity
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid ChannelId { get; set; }

        public string Method { get; set; }

        public short Status { get; set; }

        public Guid AddBy { get; set; }

        public Guid? RoleId { get; set; }

        public ChannelRole ChannelRole { get; set; }

        public Channel Channel { get; set; }

        public User User { get; set; }
    }
}