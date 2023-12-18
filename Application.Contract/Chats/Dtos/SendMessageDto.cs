namespace PBL6.Application.Contract.Chats.Dtos
{
    public class SendMessageDto
    {
        public Guid ReceiverId { get; set; }

        public string Content { get; set; }        

        public Guid? ReplyTo { get; set; }

        public bool IsChannel { get; set; }

        public List<SendFileInfoDto> Files { get; set; }
    }
}