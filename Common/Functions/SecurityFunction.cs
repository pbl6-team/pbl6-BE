using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace PBL6.Common.Functions
{
    public static class SecurityFunction
    {
        public static string GenerateRandomString(int length = 20)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*";
            char[] chars = new char[length];

            for (int i = 0; i < length; i++)
            {
                chars[i] = validChars[RandomNumberGenerator.GetInt32(validChars.Length - 1)];
            }

            return new string(chars);
        }

        public static string HashPassword(string password, string salt)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password + salt);
            byte[] hashBytes = SHA256.HashData(passwordBytes);

            var builder = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                builder.Append(hashBytes[i].ToString("x2"));
            }

            return builder.ToString();
        }

        public static string GenerationOTP(int length = 6)
        {
            try
            {
                string token = "";
                Random ran = new();
                string tmp = "0123456789";
                for (int i = 0; i < length; i++)
                {
                    token += tmp.Substring(ran.Next(0, 10), 1);
                }
                return token;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string GenerateToken(ClaimData claimData, IConfiguration configuration)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!));
            // var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var myIssuer = configuration["Jwt:Issuer"];
            var myAudience = configuration["Jwt:Audience"];

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new(CustomClaimTypes.UserId, claimData.UserId.ToString()),
                    new(CustomClaimTypes.Email, claimData.Email),
                    new(CustomClaimTypes.Username, claimData.Username),
                    new(CustomClaimTypes.IsActive, claimData.IsActive.ToString()),
                }),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(configuration["TokenTimeOut"]!)),
                Issuer = myIssuer,
                Audience = myAudience,
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        // public static string GenerateToken(string userId, string email, IConfiguration configuration)
        // {
        //     var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!));
        //     // var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        //     var myIssuer = configuration["Jwt:Issuer"];
        //     var myAudience = configuration["Jwt:Audience"];

        //     var tokenHandler = new JwtSecurityTokenHandler();
        //     var tokenDescriptor = new SecurityTokenDescriptor
        //     {
        //         Subject = new ClaimsIdentity(new Claim[]
        //         {
        //             new(ClaimTypes.NameIdentifier, userId),
        //             new(ClaimTypes.Email, email),
        //         }),
        //         Expires = DateTime.UtcNow.AddMinutes(int.Parse(configuration["TokenTimeOut"]!)),
        //         Issuer = myIssuer,
        //         Audience = myAudience,
        //         SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
        //     };

        //     var token = tokenHandler.CreateToken(tokenDescriptor);
        //     return tokenHandler.WriteToken(token);
        // }

        public static async Task<GoogleJsonWebSignature.Payload?> VerifyGoogleToken(string oauthCode, IConfiguration configuration)
        {
            try
            {
                GoogleAuthorizationCodeFlow flow = new(
                    new GoogleAuthorizationCodeFlow.Initializer
                    {
                        ClientSecrets = new ClientSecrets()
                        {
                            ClientId = configuration.GetSection("Authentication:Google:ClientId").Value!,
                            ClientSecret = configuration.GetSection("Authentication:Google:ClientSecret").Value!
                        }
                    }
                );

                var token = await flow.ExchangeCodeForTokenAsync(string.Empty, oauthCode, configuration.GetSection("Authentication:Google:RedirectUri").Value!, CancellationToken.None);
                var payload = await GoogleJsonWebSignature.ValidateAsync(token.IdToken);

                return payload;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public class ClaimData
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public bool IsActive { get; set; }
    }

    public static class CustomClaimTypes
    {
        public const string UserId = "UserId";
        public const string Email = "Email";
        public const string Username = "Username";
        public const string IsActive = "IsActive";
    }
}