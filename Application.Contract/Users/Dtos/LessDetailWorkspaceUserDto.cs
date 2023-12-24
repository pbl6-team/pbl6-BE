namespace PBL6.Application.Contract.Users.Dtos;

public class LessDetailWorkspaceUserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool? Gender { get; set; }
    public string Picture { get; set; }
    public short Status { get; set; }
}

