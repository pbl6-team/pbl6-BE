using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Users
{
    [Table("UserTokens", Schema = "User")]
    public class UserToken : FullAuditedEntity
    {
        public Guid UserId { get; set; }

        [StringLength(1000)]
        public string Token { get; set; }

        public DateTimeOffset TimeOut { get; set; }

        [StringLength(255)]
        public string RefreshToken { get; set; }
        
        public DateTimeOffset? RefreshTokenTimeOut { get; set; }

        [StringLength(50)]
        public string IpAddress { get; set; }

        [StringLength(50)]
        public string Otp { get; set; }

        public short? OtpType { get; set; }

        public DateTimeOffset ValidTo { get; set; }

        [StringLength(20)]
        public Guid DeviceId { get; set; }

        public User User { get; set; }
    }
}