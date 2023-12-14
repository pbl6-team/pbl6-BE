namespace PBL6.Application.Contract.Chats.Dtos
{
    public class GetMessageDto
    {
        public DateTimeOffset TimeCursor { get; set; }       

        public int Count { get; set; }

        public Guid? ToUserId { get; set; }
        
        public Guid? ToChannelId { get; set; }

        public Guid? ParentId { get; set; }

        public GetMessageDto()
        {
            Count = 20;
            TimeCursor = DateTimeOffset.Now;
        }
    }
}