namespace Application.Contract.Users.Dtos;

public class AdminUserDto
{
    public string Id { get; set; }    
    public string Username { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool? Gender { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Picture { get; set; }
    public short Status { get; set; }
    public DateTimeOffset BirthDay { get; set; }

}