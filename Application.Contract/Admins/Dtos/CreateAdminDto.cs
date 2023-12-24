using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Application.Contract.Admins.Dtos;

public class CreateAdminDto
{
    [StringLength(50)]
    [Required]
    [JsonPropertyName("FirstName")]
    public string FirstName { get; set; }

    [StringLength(50)]
    [Required]
    [JsonPropertyName("LastName")]
    public string LastName { get; set; }

    [StringLength(20)]
    [JsonPropertyName("Phone")]
    [RegularExpression(@"^\d{10}$")]
    public string Phone { get; set; }

    [Required]
    [StringLength(255)]
    [EmailAddress]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
    [JsonPropertyName("Email")]
    public string Email { get; set; }

    [JsonPropertyName("Gender")]
    public bool? Gender { get; set; }

    [Required]
    [JsonPropertyName("BirthDay")]
    public DateTimeOffset BirthDay { get; set; }

    [StringLength(50)]
    [Required]
    [JsonPropertyName("Username")]
    public string Username { get; set; }
}