namespace PBL6.Application.Contract.Chats.Dtos
{
    public class MessageDto
    {
        public Guid Id { get; set; }

        public Guid? ParentId { get; set; }

        public string Content { get; set; }

        public Guid SenderId { get; set; }

        public Guid ReceiverId { get; set; }

        public string SenderName { get; set; }

        public string SenderAvatar { get; set; }

        public bool IsEdited { get; set; }

        public DateTimeOffset? SendAt { get; set; }

        public string Reaction { get; set; }

        public int ChildCount { get; set; }

        public IEnumerable<Reader> Readers { get; set; }

        public bool IsChannel { get; set; }

        public Dictionary<string, int> ReactionCount { get; set; }

        public List<FileInfoDto> Files { get; set; }

        public bool IsPined { get; set; }

        public short Type { get; set; }

        public string Data { get; set; }
    }
}
