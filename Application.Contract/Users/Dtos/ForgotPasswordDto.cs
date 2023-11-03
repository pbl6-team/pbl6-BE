using System.ComponentModel.DataAnnotations;

namespace PBL6.Application.Contract.Users.Dtos
{
    public class ForgotPasswordDto
    {
        [Required]
        public string Email { get; set; }

        [Required]
        [StringLength(50)]
        public string NewPassword { get; set; }

        [Required]
        [StringLength(6)]
        public string Otp { get; set; }
    }
}