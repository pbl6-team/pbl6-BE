using System.ComponentModel.DataAnnotations;

namespace Application.Contract.Users.Dtos;

public class UpdateUserDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool? Gender { get; set; }
    public string Phone { get; set; }
    
    [EmailAddress]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
    public string Email { get; set; }
    public DateTimeOffset BirthDay { get; set; }
}