using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Users
{
    [Table("WorkspaceRoles", Schema = "Chat")]
    public class WorkspaceRole : AuditEntity
    {
        [Key]
        public Guid Id { get; set; }

        [StringLength(50)]
        [Required]
        public string Name { get; set; }

        [StringLength(100)]
        [Required]
        public string Description { get; set; }

        public string Color { get; set; }

        public IEnumerable<WorkspaceMember> Members { get; set; }

        public IEnumerable<WorkspacePermission> Permissions { get; set; }
    }
}