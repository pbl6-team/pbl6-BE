using PBL6.Application.Contract.Chats.Dtos;

namespace PBL6.Application.Contract.Notifications.Dtos
{
    public class SearchDto : Paging
    {
        public short? type { get; set; }
    }
}