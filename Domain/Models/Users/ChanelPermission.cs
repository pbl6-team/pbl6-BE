using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Users
{
    [Table("ChanelPermissions", Schema = "Chat")]
    public class ChanelPermission : AuditEntity
    {
        [Key]
        public Guid Id { get; set; }

        [StringLength(50)]
        [Required]
        public string Name { get; set; }

        [StringLength(100)]
        [Required]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        [Required]
        public string Code { get; set; }
        
        public IEnumerable<ChanelRole> ChanelRoles { get; set; }
    }
}