using System.ComponentModel.DataAnnotations;

namespace PBL6.Domain.Models.Users
{
    public class MessageTracking
    {
        [Key]
        public Guid Id { get; set; }

        public Guid MessageId { get; set; }
        
        public Guid UserId { get; set; }

        public bool IsRead { get; set; }

        public DateTimeOffset? ReadTime { get; set; }

        public bool IsDeleted { get; set; }

        public DateTimeOffset? DeletedTime { get; set; }

        public Message Message { get; set; }

        public User User { get; set; }

        public string Reaction { get; set; }
    }
}