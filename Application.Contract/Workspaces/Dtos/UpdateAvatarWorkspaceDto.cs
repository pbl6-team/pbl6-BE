using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PBL6.Application.Contract.Workspaces.Dtos
{
    public class UpdateAvatarWorkspaceDto
    {
        public IFormFile Avatar { get; set; }
    }
}   