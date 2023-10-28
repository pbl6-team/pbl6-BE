using System.ComponentModel.DataAnnotations;

namespace PBL6.Application.Contract.Users.Dtos
{
    public class ChangePasswordDto
    {
        [Required]
        [StringLength(50)]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(50)]
        public string NewPassword { get; set; }

        [Required]
        [StringLength(6)]
        public string OTP { get; set; }
    }
}