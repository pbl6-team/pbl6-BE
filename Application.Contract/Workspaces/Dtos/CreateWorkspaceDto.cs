
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PBL6.Application.Contract.Workspaces.Dtos
{
    public class CreateWorkspaceDto
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        public IFormFile Avatar { get; set; }
    }
}