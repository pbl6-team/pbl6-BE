using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Users
{
    [Table("Notifications", Schema = "Notifications")]
    public class Notification : FullAuditedEntity
    {
        [StringLength(255)]
        [Required]
        public string Title { get; set; }

        [StringLength(255)]
        [Required]
        public string Content { get; set; }

        public DateTimeOffset TimeToSend { get; set; }

        [Required]
        public short Status { get; set; }

        public string RefId { get; set; }

        [Required]
        public short Type { get; set; }

        public IEnumerable<UserNotification> UserNotifications { get; set; }
    }
}