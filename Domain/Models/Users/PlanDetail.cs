using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Users
{

    [Table("PlanDetails", Schema = "Users")]
    public class PlanDetail : AuditEntity
    {
        [Key]
        public Guid Id { get; set; }

        [StringLength(50)]
        [Required]
        public string Code { get; set; }

        [StringLength(255)]
        [Required]
        public string Description { get; set; }

        public int Limit { get; set; }

        public Guid PlanId { get; set; }
        
        public Plan Plan { get; set; }
    }
}