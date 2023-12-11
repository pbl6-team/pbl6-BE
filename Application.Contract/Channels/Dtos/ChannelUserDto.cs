using PBL6.Application.Contract.Channels.Dtos;

namespace Application.Contract.Channels.Dtos;

public class ChannelUserDto
{
    public string Id { get; set; }    
    public string Username { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool? Gender { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Picture { get; set; }
    public DateTimeOffset BirthDay { get; set; }
    public ChannelRoleDto Role { get; set; }

}