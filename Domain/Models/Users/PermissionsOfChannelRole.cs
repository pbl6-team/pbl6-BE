using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Users
{
    [Table("PermissionsOfChannelRoles", Schema = "Chat")]
    public class PermissionsOfChannelRole : FullAuditedEntity
    {
        [Required]
        public Guid ChannelRoleId { get; set; }
        
        [Required]
        public Guid PermissionId { get; set; }

        public ChannelPermission Permission { get; set; }

        public ChannelRole ChannelRole { get; set; }

        [Required]
        public bool IsEnabled { get; set; }
    }
}