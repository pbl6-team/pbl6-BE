using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Users
{
    [Table("WorkspaceRoles", Schema = "Chat")]
    public class WorkspaceRole : FullAuditedEntity
    {
        [StringLength(50)]
        [Required]
        public string Name { get; set; }

        [StringLength(100)]
        [Required]
        public string Description { get; set; }

        public string Color { get; set; }

        public Guid WorkspaceId { get; set; }

        public Workspace Workspace { get; set; }

        public ICollection<WorkspaceMember> Members { get; set; }

        public ICollection<WorkspacePermission> Permissions { get; set; }
    }
}