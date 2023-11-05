using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Users
{
    [Table("ChanelRoles", Schema = "Chat")]
    public class ChanelRole : FullAuditedEntity
    {
        [StringLength(50)]
        [Required]
        public string Name { get; set; }

        [StringLength(100)]
        [Required]
        public string Description { get; set; }

        public string Color { get; set; }

        public IEnumerable<ChannelMember> Members { get; set; }

        public IEnumerable<ChanelPermission> Permissions { get; set; }
    }
}