namespace PBL6.Application.Contract.Users.Dtos
{
    public class TokenData
    {
        public Guid UserId { get; set; }

        public string Email { get; set; }

        public string Token { get; set; }

        public DateTimeOffset TokenTimeOut { get; set; }

        public string RefreshToken { get; set; }

        public DateTimeOffset RefreshTokenTimeout { get; set; }

        public string TokenType { get; set; }
    }
}
