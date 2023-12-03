using PBL6.Application.Contract.Workspaces.Dtos;

namespace PBL6.Application.Contract.Channels.Dtos
{
    public class ChannelRoleDto
    {
         public Guid Id { get; set; }
        
        public string Name { get; set; }

        public string Description { get; set; }
        
        public string Color { get; set; }

        public int NumberOfMembers { get; set; }

        public List<PermissionDto> Permissions { get; set; }
    }
}
