using System.ComponentModel.DataAnnotations;

namespace PBL6.Domain.Models.Common
{
    public class FullAuditedEntity : AuditedEntity
    {
        [Key]
        public Guid Id { get; set; }
    }
}