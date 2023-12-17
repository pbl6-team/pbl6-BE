using Microsoft.AspNetCore.Http;

namespace PBL6.Application.Contract.Users.Dtos;

public class UpdateUserPictureDto
{
    public IFormFile Picture { get; set; }
}