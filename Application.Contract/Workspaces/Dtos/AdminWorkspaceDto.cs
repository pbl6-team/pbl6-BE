using PBL6.Application.Contract;
namespace Application.Contract.Workspaces.Dtos;

public class AdminWorkspaceDto : FullAuditedDto
{
    public string Name { get; set; }

    public string Description { get; set; }

    public string AvatarUrl { get; set; }

    public Guid OwnerId { get; set; }

    public string OwnerName { get; set; }

    public short Status { get; set; }
}