using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Users
{
    [Table("User", Schema = "Users")]
    public class User : AuditEntity
    {
        [Key]
        public Guid Id { get; set; }

        [StringLength(30)]
        [Required]
        public string UserName { get; set; }

        [StringLength(50)]
        [Required]
        public string Email { get; set; }

        [StringLength(50)]
        [Required]
        public string Password { get; set; }

        [StringLength(50)]
        [Required]
        public string PasswordSalt { get; set; }

        [Required]
        public bool IsActive { get; set; }

        public DateTimeOffset? LastLoginAt { get; set; }

        public DateTimeOffset? LastLogoutAt { get; set; }

        public Guid InfoId { get; set; }

        public UserInfo Information { get; set; }

        public IEnumerable<UserToken> UserTokens { get; set; }

        public IEnumerable<UserNotification> UserNotifications { get; set; }
    }
}