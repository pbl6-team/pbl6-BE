using Microsoft.AspNetCore.Http;

namespace Application.Contract.Users.Dtos;

public class UpdateUserPictureDto
{
    public IFormFile Picture { get; set; }
}