namespace PBL6.Application.Contract.Workspaces.Dtos
{
    public class ChannelDto : FullAuditedDto
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string OwnerId { get; set; }

        public Guid WorkspaceId { get; set; }

        public IEnumerable<Guid> ChannelMembers { get; set; }

        public Guid? CategoryId { get; set; }
    }
}