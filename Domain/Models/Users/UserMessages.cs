using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Users
{
    [Table("UserMessagess", Schema = "Chat")]
    public class UserMessages : AuditEntity
    {
         [Key]
        public Guid Id { get; set; }

        public Guid ToUserId { get; set; }

        public string RefIds { get; set; }
        [Required]
        public bool IsSeen { get; set; }

        public Guid? ParentId { get; set; }
    }
}