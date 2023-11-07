using Microsoft.AspNetCore.Http;

namespace PBL6.Application.Contract.Common
{
    public interface IFileService
    {
        Task<string> UploadImageToImgbb(IFormFile file, Guid id);
    }
}