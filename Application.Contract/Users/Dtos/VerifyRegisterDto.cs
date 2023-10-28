using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PBL6.Application.Contract.Users.Dtos
{
    public class VerifyRegisterDto
    {
        /// <summary>
        /// Otp
        /// </summary>
        /// <example>123456</example>
        [StringLength(6)]
        [JsonPropertyName("Otp")]
        public string Otp { get; set; }
    }
}