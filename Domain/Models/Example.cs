using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models
{
    public class Example : FullAuditedEntity
    {
        [StringLength(30)]
        [Required]
        public string Name { get; set; }

        [StringLength(10)]
        [Required]
        public string Phone { get; set; }

        [Required]
        [StringLength(255)]
        public string Address { get; set; }
    }
}