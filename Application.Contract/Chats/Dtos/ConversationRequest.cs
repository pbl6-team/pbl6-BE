namespace PBL6.Application.Contract.Chats.Dtos
{
    public class ConversationRequest : Paging
    {
        public string Search { get; set; }

        public ConversationRequest()
        {
            Search = "";
        }
    }
}