using System.ComponentModel.DataAnnotations;

namespace PBL6.Application.Contract.Workspaces.Dtos
{
    public class UpdateWorkspaceDto
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}