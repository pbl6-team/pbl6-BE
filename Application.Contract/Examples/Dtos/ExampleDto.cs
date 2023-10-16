using System.ComponentModel.DataAnnotations;

namespace PBL6.Application.Contract.Examples.Dtos
{
    public class ExampleDto : FullAuditedDto
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