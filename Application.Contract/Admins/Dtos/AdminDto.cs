namespace Application.Contract.Admins.Dtos;

public class AdminDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Phone {get; set;}
    public string Email {get; set;}
    public string Username {get; set;}
    public short Status { get; set; }
}

