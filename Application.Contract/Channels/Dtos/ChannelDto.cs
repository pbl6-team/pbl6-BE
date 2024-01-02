namespace PBL6.Application.Contract.Workspaces.Dtos
{
    public class ChannelDto : FullAuditedDto
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public Guid OwnerId { get; set; }

        public Guid WorkspaceId { get; set; }
        
        public short Category { get; set; }
        
        public IEnumerable<Guid> ChannelMembers { get; set; }
    }
}