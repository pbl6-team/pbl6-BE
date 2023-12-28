namespace PBL6.Application.Contract.Meetings.Dtos
{
    public class CreateMeetingDto
    {
        public string Name { get; set; }

        public string SessionId { get; set; }

        public string Password { get; set; }

        public DateTimeOffset TimeStart { get; set; }

        public DateTimeOffset TimeEnd { get; set; }

        public string Description { get; set; }

        public short Type { get; set; }

        public bool IsSendEmail { get; set; }

        public bool IsNotify { get; set; }

        public Guid WorkspaceId { get; set; }

        public Guid? ChannelId { get; set; }

        public List<Guid> MemberIds { get; set; }
    }
}
