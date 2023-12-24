using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PBL6.Application.Contract.Users.Dtos
{
    public class UserRegisterDto
    {
        /// <summary>
        /// Email
        /// </summary>
        /// <example>abc@gmail.com</example>
        [Required]
        [StringLength(255)]
        [EmailAddress]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
        [JsonPropertyName("Email")]
        public string Email { get; set; }

        /// <summary>
        /// Tên đăng nhập
        /// </summary>
        /// <example>nhamcotdo</example>
        [StringLength(30)]
        [Required]
        [JsonPropertyName("Username")]
        public required string Username { get; set; }
        

        /// <summary>
        /// Password after endcode MD5
        /// </summary>
        /// <example>0e7517141fb53f21ee439b355b5a1d0a</example>
        [Required]
        [StringLength(50)]
        [JsonProperty("password")]
        [JsonPropertyName("Password")]
        public required string Password { get; set; }

        /// <summary>
        /// Tên
        /// </summary>
        /// <example>Nham</example>
        [StringLength(50)]
        [Required]
        [JsonPropertyName("FirstName")]
        public string FirstName { get; set; }

        /// <summary>
        /// Họ
        /// </summary>
        /// <example>Nguyen</example>
        [StringLength(50)]
        [Required]
        [JsonPropertyName("LastName")]
        public string LastName { get; set; }

        /// <summary>
        /// Số điện thoại
        /// </summary>
        /// <example>0523061xxx</example>
        [StringLength(20)]
        [JsonPropertyName("Phone")]
        [RegularExpression(@"^\d{10}$")]
        public string Phone { get; set; }

        /// <summary>
        /// Giới tính, true: nữ, false: nam, null: Bí mật
        /// </summary>
        [JsonPropertyName("Gender")]
        public bool? Gender { get; set; }

        /// <summary>
        /// Ngày sinh
        /// </summary>
        [Required]
        [JsonPropertyName("BirthDay")]
        public DateTimeOffset BirthDay { get; set; }
    }
}