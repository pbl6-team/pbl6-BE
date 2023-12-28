namespace PBL6.Application.Contract.Meetings.Dtos
{
    public class MemberOfMeetingDto
    {
        public Guid UserId { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public bool IsHost { get; set; }
    }
}