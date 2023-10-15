using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Users
{
    [Table("WorkspaceMembers", Schema = "Chat")]
    public class WorkspaceMember : AuditEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        public User User { get; set; }

        [Required]
        public Guid WorkspaceId { get; set; }

        public Workspace Workspace { get; set; }

        public string Method { get; set; }

        public Guid AddBy { get; set; }

        public Guid RoleId { get; set; }

        public WorkspaceRole WorkspaceRole { get; set; }
    }
}