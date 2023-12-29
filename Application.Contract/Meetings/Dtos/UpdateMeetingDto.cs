namespace PBL6.Application.Contract.Meetings.Dtos
{
    public class UpdateMeetingDto
    {
        public string Name { get; set; }

        public DateTimeOffset TimeStart { get; set; }

        public DateTimeOffset TimeEnd { get; set; }

        public string Password { get; set; }

        public string Description { get; set; }

        public short Type { get; set; }

        public List<Guid> MemberIds { get; set; }
    }
}