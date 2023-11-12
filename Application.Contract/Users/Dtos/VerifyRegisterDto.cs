using System.ComponentModel.DataAnnotations;

namespace PBL6.Application.Contract.Users.Dtos
{
    public class VerifyRegisterDto
    {
        /// <summary>
        /// Otp
        /// </summary>
        /// <example>123456</example>
        [StringLength(6)]
        public string Otp { get; set; }
    }
}