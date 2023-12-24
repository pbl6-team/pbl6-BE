using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Application.Contract.Admins.Dtos;

public class UpdateAdminDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Phone { get; set; }

    [StringLength(255)]
    [EmailAddress]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
    public string Email { get; set; }

    public bool? Gender { get; set; }

    public DateTimeOffset BirthDay { get; set; }

}