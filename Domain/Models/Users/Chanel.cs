using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Users
{
    [Table("Chanels", Schema = "Chat")]
    public class Chanel : FullAuditedEntity
    {
        [StringLength(50)]
        [Required]
        public string Name { get; set; }

        public Guid? ParentId { get; set; }

        [StringLength(255)]
        [Required]
        public string Description { get; set; }

        [Required]
        public Guid WorkspaceId { get; set; }

        public Workspace Workspace { get; set; }
    }
}