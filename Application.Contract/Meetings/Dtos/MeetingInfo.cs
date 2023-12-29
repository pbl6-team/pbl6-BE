namespace PBL6.Application.Contract.Meetings.Dtos
{
    public class MeetingInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string SessionId { get; set; }
        public string Password { get; set; }
        public short Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Guid WorkspaceId { get; set; }
        public string WorkspaceName { get; set; }
        public Guid ChannelId { get; set; }
        public string ChannelName { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
