using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Admins
{
    [Table("Roles", Schema = "Admin")]
    public class Role : FullAuditedEntity
    {


        [StringLength(50)]
        [Required]
        public string Code { get; set; }

        [StringLength(100)]
        [Required]
        public string Name { get; set; }

        [StringLength(255)]
        [Required]
        public string Description { get; set; }

        public ICollection<Function> Functions { get; set; }

        public ICollection<AdminAccount> AdminAccounts { get; set; }
    }
}