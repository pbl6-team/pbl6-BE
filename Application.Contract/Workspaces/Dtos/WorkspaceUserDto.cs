using PBL6.Application.Contract.Workspaces.Dtos;

namespace Application.Contract.Workspaces.Dtos;

public class WorkspaceUserDto
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
    public WorkspaceRoleDto Role { get; set; }

}