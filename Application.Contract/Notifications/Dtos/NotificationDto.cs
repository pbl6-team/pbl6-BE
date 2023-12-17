namespace PBL6.Application.Contract.Notifications.Dtos
{
    public class NotificationDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public bool IsRead { get; set; }

        public short Type { get; set; }

        public string Data { get; set; }
    }
}