namespace PBL6.Application.Contract.Chats.Dtos
{
    public class Reader
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Avatar { get; set; }

        public DateTimeOffset ReadTime { get; set; }
    }
}
