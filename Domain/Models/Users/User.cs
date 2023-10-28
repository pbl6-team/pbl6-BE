using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Users
{
    [Table("Users", Schema = "User")]
    public class User : FullAuditedEntity
    {
        [StringLength(30)]
        [Required]
        public string Username { get; set; }

        [StringLength(50)]
        [Required]
        public string Email { get; set; }

        [StringLength(255)]
        public string Password { get; set; }

        [StringLength(50)]
        public string PasswordSalt { get; set; }

        [Required]
        public bool IsActive { get; set; }

        public DateTimeOffset? LastLoginAt { get; set; }

        public DateTimeOffset? LastLogoutAt { get; set; }

        public Guid InfoId { get; set; }

        public UserInfo Information { get; set; }

        public ICollection<UserToken> UserTokens { get; set; }

        public IEnumerable<UserNotification> UserNotifications { get; set; }
    }
}