using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Admins
{
    [Table("AdminAccounts", Schema = "Admin")]
    public class AdminAccount : FullAuditedEntity
    {
        [StringLength(50)]
        [Required]
        public string Username { get; set; }

        [StringLength(100)]
        [Required]
        public string Password { get; set; }

        [StringLength(30)]
        [Required]
        public string PasswordSalt { get; set; }

        [StringLength(50)]
        [Required]
        public string Email { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [StringLength(50)]
        public string IpAddress { get; set; }

        public DateTimeOffset LastLogin { get; set; }

        public DateTimeOffset LastLogout { get; set; }

        public Guid InfoId { get; set; }

        public AdminInfo Information { get; set; }

        public ICollection<Role> Roles { get; set; }

        public ICollection<AdminToken> AdminTokens { get; set; }
    }
}