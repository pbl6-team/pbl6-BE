namespace PBL6.Application.Contract
{
    public class AuditedDto
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