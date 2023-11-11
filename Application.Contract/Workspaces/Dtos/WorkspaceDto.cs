namespace PBL6.Application.Contract.Workspaces.Dtos
{
    public class WorkspaceDto : FullAuditedDto
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string AvatarUrl { get; set; }
 
        public Guid OwnerId { get; set; }
 
        public IEnumerable<Guid> Members { get; set; }
 
        public IEnumerable<Guid> Channels { get; set; }
    }
}