using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Admins
{
    [Table("RolesOfAdmins", Schema = "Admin")]
    public class RolesOfAdmin : FullAuditedEntity
    {
        public Guid RoleId { get; set; }

        public Guid AdminId { get; set; }
    }
}