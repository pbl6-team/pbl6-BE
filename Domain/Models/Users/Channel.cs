using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Users
{
    [Table("Channels", Schema = "Chat")]
    public class Channel : FullAuditedEntity
    {
        [StringLength(50)]
        [Required]
        public string Name { get; set; }

        public Guid OwnerId { get; set; }
        
        public Guid? CategoryId { get; set; }

        [StringLength(255)]
        [Required]
        public string Description { get; set; }

        [Required]
        public Guid WorkspaceId { get; set; }

        public Workspace Workspace { get; set; }

        public ICollection<ChannelMember> ChannelMembers { get; set; }

        public ICollection<ChannelRole> ChannelRoles { get; set; }
    }
}