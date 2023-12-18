namespace PBL6.Application.Contract.Chats.Dtos
{
    public class GetPinMessageDto : Paging
    {
        public Guid? ToUserId { get; set; }
        
        public Guid? ToChannelId { get; set; }
    }
}
