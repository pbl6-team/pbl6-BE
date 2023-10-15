using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Users
{
    [Table("Workspaces", Schema = "Chat")]
    public class Workspace : AuditEntity
    {
        [Key]
        public Guid Id { get; set; }

        [StringLength(50)]
        [Required]
        public string Name { get; set; }

        [StringLength(255)]
        [Required]
        public string Description { get; set; }

        [Required]
        public Guid OwnerId { get; set; }

        public User Owner { get; set; }

        public IEnumerable<Chanel> Chanels { get; set; }

        public IEnumerable<WorkspaceMember> Members { get; set; }
    }
}