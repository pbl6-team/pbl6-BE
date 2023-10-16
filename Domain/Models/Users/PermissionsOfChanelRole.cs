using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Users
{
    [Table("PermissionsOfChanelRoles", Schema = "Chat")]
    public class PermissionsOfChanelRole : FullAuditedEntity
    {
        [Required]
        public Guid ChanelRoleId { get; set; }
        
        [Required]
        public Guid PermissionId { get; set; }

        [Required]
        public bool IsEnable { get; set; }
    }
}