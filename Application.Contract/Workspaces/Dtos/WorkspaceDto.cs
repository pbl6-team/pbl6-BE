namespace PBL6.Application.Contract.Workspaces.Dtos
{
    public class WorkspaceDto : FullAuditedDto
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string AvatarUrl { get; set; }
    }
}