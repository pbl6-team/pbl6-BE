using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Users
{
    [Table("ChannelCategories", Schema = "Chat")]
    public class ChannelCategory : FullAuditedEntity
    {
        [Required]
        [StringLength(255)]
        public string CategoryName { get; set; }
    }
}