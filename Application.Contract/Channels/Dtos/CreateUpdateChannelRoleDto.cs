using PBL6.Application.Contract.Workspaces.Dtos;

namespace PBL6.Application.Contract.Channels.Dtos
{
    public class CreateUpdateChannelRoleDto
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string  Color { get; set; }

        public List<CreateUpdatePermissionDto> Permissions { get; set; }        
    }
}