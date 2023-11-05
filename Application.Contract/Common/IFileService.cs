using Microsoft.AspNetCore.Http;

namespace PBL6.Application.Contract.Common
{
    public interface IFileService
    {
        Task SaveFileAsync(IFormFile file, string filePath);
    }
}