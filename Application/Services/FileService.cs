using Microsoft.AspNetCore.Http;
using PBL6.Application.Contract.Common;

namespace PBL6.Application.Services
{
    public class FileService : IFileService
    {
        public async Task SaveFileAsync(IFormFile file, string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write))
            {
                await file.CopyToAsync(fileStream);
            }
        }
    }
}