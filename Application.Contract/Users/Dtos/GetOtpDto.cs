using System.ComponentModel.DataAnnotations;

namespace PBL6.Application.Contract.Users.Dtos
{
    public class GetOtpDto
    {
        [Required]
        public short  OtpType { get; set; }

        [Required]
        public string Email { get; set; }
    }
}