using Microsoft.AspNetCore.Http;

namespace PBL6.Common.Functions;

public static class CommonFunctions
{
    public static string GetFileExtension(string fileName) => Path.GetExtension(fileName);

    public static async Task<byte[]> GetBytesAsync(this IFormFile formFile)
    {
        using var memoryStream = new MemoryStream();
        await formFile.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }
}