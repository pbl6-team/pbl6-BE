using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace PBL6.Application.Contract.Users.Dtos
{
    public class UserLoginDto
    {
        /// <summary>
        /// Email or username
        /// </summary>
        [Required()]
        [StringLength(255)]
        public string Username { get; set; }

        /// <summary>
        /// Password after endcode MD5
        /// </summary>
        /// <example>0e7517141fb53f21ee439b355b5a1d0a</example>
        [Required()]
        [StringLength(50)]
        [JsonProperty("password")]
        public string Password { get; set; }
    }
}