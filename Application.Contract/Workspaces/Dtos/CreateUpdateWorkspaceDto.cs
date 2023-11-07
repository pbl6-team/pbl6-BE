
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PBL6.Application.Contract.Workspaces.Dtos
{
    public class CreateUpdateWorkspaceDto
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string Description { get; set; }

        public IFormFile Avatar { get; set; }
    }
}