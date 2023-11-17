namespace PBL6.Application.Contract.Workspaces.Dtos
{
    public class PermissionDto
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; }

        public string Code { get; set; }

        public string Description { get; set; }

        public bool IsEnabled { get; set; }
    }
}
