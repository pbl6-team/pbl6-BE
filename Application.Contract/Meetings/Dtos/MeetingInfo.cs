using PBL6.Application.Contract.Users.Dtos;
using PBL6.Domain.Models.Users;

namespace PBL6.Application.Contract.Meetings.Dtos
{
    public class MeetingInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description {get; set;}
        public List<UserDetailDto> Participants { get; set; }
        public string SessionId { get; set; }
        public string Password { get; set; }
        public short Status { get; set; }
        public DateTimeOffset TimeStart { get; set; }
        public DateTimeOffset TimeEnd { get; set; }
        public Guid WorkspaceId { get; set; }
        public string WorkspaceName { get; set; }
        public Guid ChannelId { get; set; }
        public string ChannelName { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
