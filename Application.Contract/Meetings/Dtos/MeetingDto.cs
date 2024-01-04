namespace PBL6.Application.Contract.Meetings.Dtos
{
    public class MeetingDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string SessionId { get; set; }

        public string Password { get; set; }

        public DateTimeOffset TimeStart { get; set; }

        public DateTimeOffset TimeEnd { get; set; }

        public string Description { get; set; }

        public Guid? ChannelId { get; set; }

        public List<MemberOfMeetingDto> Members { get; set; }   

        public short Status { get; set; }
    }
}