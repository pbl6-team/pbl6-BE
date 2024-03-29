using System.ComponentModel.DataAnnotations.Schema;
using PBL6.Domain.Models.Common;

namespace PBL6.Domain.Models.Users
{
    [Table("Messages", Schema = "Chat")]
    public class Message : FullAuditedEntity
    {
        public string Content { get; set; }

        public Guid? ToUserId { get; set; }

        public Guid? ToChannelId { get; set; }

        public Channel ToChannel { get; set; }

        public Guid? ParentId { get; set; }

        public bool IsEdited { get; set; }

        public bool IsPined { get; set; }

        public Message Parent { get; set; }

        public ICollection<Message> Children { get; set; }

        public ICollection<MessageTracking> MessageTrackings { get; set; }

        public User Sender { get; set; }

        public User Receiver { get; set; }

        public ICollection<FileOfMessage> Files { get; set; }

        public short Type { get; set; }

        public string Data { get; set; }
    }
}