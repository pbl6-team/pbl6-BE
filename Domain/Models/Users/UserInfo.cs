using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Users
{

    [Table("UserInfos", Schema = "Users")]
    public class UserInfo : FullAuditedEntity
    {
        [StringLength(50)]
        [Required]
        public string FirstName { get; set; }

        [StringLength(50)]
        [Required]
        public string LastName { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        public short Status { get; set; }

        public bool? Gender { get; set; }

        [Required]
        public DateTimeOffset BirthDay { get; set; }

        public User User { get; set; }
    }
}