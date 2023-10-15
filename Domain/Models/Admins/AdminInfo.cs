using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Admins
{
    [Table("AdminInfos", Schema = "Admin")]
    public class AdminInfo : AuditEntity
    {
        [Key]
        public Guid Id { get; set; }

        [StringLength(50)]
        [Required]
        public string FirstName { get; set; }

        [StringLength(50)]
        [Required]
        public string LastName { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(255)]
        public string Address { get; set; }

        [Required]
        public DateTimeOffset BirthDate { get; set; }

        public bool? Gender { get; set; }

        public short Status { get; set; }

        public AdminAccount AdminAccount { get; set; }
    }
}