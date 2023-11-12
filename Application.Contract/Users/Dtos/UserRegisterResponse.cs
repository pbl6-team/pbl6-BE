using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace PBL6.Application.Contract.Users.Dtos
{
    public class UserRegisterResponse
    {
        /// <summary>
        /// Token
        /// </summary>
        /// <example>jwt token</example>
        [Required]
        public string Token { get; set; }

        public DateTimeOffset TokenTimeOut { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        /// <example>123456@gmail.com</example>
        public string Email { get; set; }
    }
}