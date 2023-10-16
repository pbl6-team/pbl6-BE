using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Users
{

    [Table("Plans", Schema = "Users")]
    public class Plan : FullAuditedEntity
    {
        [StringLength(100)]
        [Required]
        public string Name { get; set; }

        [StringLength(255)]
        [Required]
        public string Description { get; set; }

        public short Status { get; set; }
    }
}