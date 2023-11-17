using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Users
{
    [Table("PermissionsOfWorkspaceRoles", Schema = "Chat")]
    public class PermissionsOfWorkspaceRole : FullAuditedEntity
    {
        [Required]
        public Guid WorkspaceRoleId { get; set; }

        [Required]
        public Guid PermissionId { get; set; }

        public WorkspacePermission Permission { get; set; }

        public WorkspaceRole WorkspaceRole { get; set; }
    }
}
