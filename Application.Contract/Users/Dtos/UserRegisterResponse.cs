using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PBL6.Application.Contract.Users.Dtos
{
    public class UserRegisterResponse
    {
        /// <summary>
        /// Token
        /// </summary>
        /// <example>jwt token</example>
        [Required]
        [JsonPropertyName("Token")]
        public string Token { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        /// <example>123456@gmail.com</example>
        [JsonPropertyName("Email")]
        public string Email { get; set; }
    }
}