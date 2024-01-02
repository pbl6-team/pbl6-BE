namespace PBL6.Application.Contract.Chats.Dtos
{
    public class GetFileDto : Paging
    {
        public Guid? ToUserId { get; set; }
        
        public Guid? ToChannelId { get; set; }

        public short Type { get; set; }
    }
}
