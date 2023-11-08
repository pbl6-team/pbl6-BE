using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PBL6.Application.Contract.Workspaces.Dtos
{
    public class UpdateWorkspaceDto
    {
        public string Name { get; set; }

        public string Description { get; set; }
    }
}