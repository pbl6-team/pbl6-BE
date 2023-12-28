namespace PBL6.Application.Contract.Meetings.Dtos
{
    public class CallDto
    {
        public Guid Id { get; set; }
        public string SessionId { get; set; }
        public string Password { get; set; }
        public short Status { get; set; }
    }
}
