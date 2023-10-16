using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Users
{
    [Table("UserNotifications", Schema = "Notifications")]
    public class UserNotification : FullAuditedEntity
    {
        public DateTimeOffset SendAt { get; set; }

        [Required]
        public short Status { get; set; }

        public Guid? PushId { get; set; }

        public string RefId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        public User User { get; set; }

        public Guid NotificationId { get; set; }

        public Notification Notification { get; set; }
    }
}