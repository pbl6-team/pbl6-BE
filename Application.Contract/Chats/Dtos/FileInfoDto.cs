namespace PBL6.Application.Contract.Chats.Dtos
{
    public class FileInfoDto
    {
        public Guid Id { get; set; }
        
        public string Url { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}