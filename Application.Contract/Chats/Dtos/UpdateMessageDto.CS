namespace PBL6.Application.Contract.Chats.Dtos
{
    public class UpdateMessageDto
    {
        public Guid Id { get; set; }

        public string Content { get; set; }

        public bool IsChannel { get; set; }
    }
}
