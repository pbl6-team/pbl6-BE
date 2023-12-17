namespace PBL6.Application.Contract.ExternalServices.Notifications.Dtos
{
    public class NewMessageData
    {
        public Guid ConversationId { get; set; }

        public bool IsChannel { get; set; }

        public Guid SenderId { get; set; }

        public string SenderName { get; set; }

        public string SenderAvatar { get; set; }

        public string Content { get; set; }
    }
}