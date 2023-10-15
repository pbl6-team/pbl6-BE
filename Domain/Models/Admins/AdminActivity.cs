using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Admins
{
    [Table("AdminActivities", Schema = "Admin")]
    public class AdminActivity : AuditEntity
    {
        [Key]
        public Guid Id { get; set; }

        [StringLength(100)]
        [Required]
        public string Action { get; set; }

        [StringLength(255)]
        public string Object { get; set; }

        [StringLength(50)]
        public string IpAdress { get; set; }

        public Guid DeviceId { get; set; }

        public Guid AdminId { get; set; }

        public AdminAccount AdminAccount { get; set; }
    }
}