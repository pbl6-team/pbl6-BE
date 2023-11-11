using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Users
{
    [Table("Workspaces", Schema = "Chat")]
    public class Workspace : FullAuditedEntity
    {
        [StringLength(50)]
        [Required]
        public string Name { get; set; }

        [StringLength(255)]
        [Required]
        public string Description { get; set; }

        [Required]
        public string AvatarUrl { get; set; }

        [Required]
        public Guid OwnerId { get; set; }

        public User Owner { get; set; }

        public ICollection<Channel> Channels { get; set; }

        public ICollection<WorkspaceMember> Members { get; set; }

        public ICollection<WorkspaceRole> WorkspaceRoles { get; set; }
    }
}