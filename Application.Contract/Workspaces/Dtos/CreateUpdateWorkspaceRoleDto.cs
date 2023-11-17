namespace PBL6.Application.Contract.Workspaces.Dtos
{
    public class CreateUpdateWorkspaceRoleDto
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string  Color { get; set; }

        public List<CreateUpdatePermissionDto> Permissions { get; set; }        
    }
}