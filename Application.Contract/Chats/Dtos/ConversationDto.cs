namespace PBL6.Application.Contract.Chats.Dtos
{
    public class ConversationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string LastMessage { get; set; }
        public DateTimeOffset LastMessageTime { get; set; }
        public string LastMessageSender { get; set; }
        public string LastMessageSenderAvatar { get; set; }
        public bool IsRead { get; set; }
        public bool IsChannel { get; set; }
        public string Avatar { get; set; }
        public bool IsOnline { get; set; }
    }
}