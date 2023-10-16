namespace PBL6.Domain.Models.Common
{
    public class AuditedEntity
    {
        public Guid? CreatedBy { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public Guid? UpdatedBy { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }

        public Guid? DeletedBy { get; set; }

        public DateTimeOffset? DeletedAt { get; set; }
    }
}