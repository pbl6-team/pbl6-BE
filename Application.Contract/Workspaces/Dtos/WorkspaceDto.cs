using PBL6.Application.Contract.Users.Dtos;

namespace PBL6.Application.Contract.Workspaces.Dtos
{
    public class WorkspaceDto : FullAuditedDto
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string AvatarUrl { get; set; }
 
        public Guid OwnerId { get; set; }

        public short Status { get; set; }
        
        public IEnumerable<UserDto> Members { get; set; }
 
        public IEnumerable<Guid> Channels { get; set; }
    }
}