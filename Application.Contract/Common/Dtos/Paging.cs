namespace PBL6.Application.Contract.Chats.Dtos
{
    public class Paging
    {
        public int Offset { get; set; }

        public int Limit { get; set; }

        public Paging()
        {
            Offset = 0;
            Limit = 10;
        }
    }
}