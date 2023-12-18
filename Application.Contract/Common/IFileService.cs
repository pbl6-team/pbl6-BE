using Microsoft.AspNetCore.Http;

namespace PBL6.Application.Contract.Common
{
    public interface IFileService
    {
        Task<string> UploadImageToImgbb(IFormFile file, Guid id);
        Task UploadFileAsync(string fileName, Stream stream, string contentType = "");
        Task<(Stream, string)> DownloadFileAsync(string fileName);
        Task<bool> FileExistsAsync(string fileName);
        Task DeleteFileAsync(List<string> fileNames);
        Task CopyFileAsync(string fileName, string newFileName);
        Task DeleteFileUrlAsync(List<string> filePath);
        string GetBaseUrl();
        Task<string> UploadFileGetUrlAsync(string fileName, Stream stream, string contentType = "");
    }
}
