namespace PBL6.Application.Contract.Users.Dtos
{
    public class UserNotInChannelDto
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool? Gender { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Picture { get; set; }
        public DateTimeOffset BirthDay { get; set; }
        public bool IsInvited { get; set; }
    }
}
