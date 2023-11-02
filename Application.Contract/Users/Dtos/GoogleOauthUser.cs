namespace PBL6.Application.Contract.Users.Dtos
{
    public class GoogleOauthUser
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Picture { get; set; }
        public bool EmailVerified { get; set; }
    }
}