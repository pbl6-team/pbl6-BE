using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Admins
{
    [Table("Functions", Schema = "Admin")]
    public class Function : FullAuditedEntity
    {

        [Required]
        public string Code { get; set; }

        [StringLength(100)]
        [Required]
        public string Name { get; set; }

        [StringLength(255)]
        [Required]
        public string Description { get; set; }

        public IEnumerable<Role> Roles { get; set; }
    }
}