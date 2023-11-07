using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using PBL6.Application.Contract.Common;
using PBL6.Common.Functions;

namespace PBL6.Application.Services
{
    public class FileService : IFileService
    {
        private const string ImgbbAPI = "0b3a1a01592a719072a36436ba3f503a";
        public async Task<string> UploadImageToImgbb(IFormFile file, Guid id)
        {
            var client = new HttpClient();
            var url = $"https://api.imgbb.com/1/upload?key={ImgbbAPI}";
            var content = new MultipartFormDataContent();
            var b64 = Convert.ToBase64String(await CommonFunctions.GetBytesAsync(file));

            content.Add(new StringContent(b64), "image");

            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            var response = await client.SendAsync(request);
            return JObject.Parse(await response.Content.ReadAsStringAsync())["data"]["url"].ToString();
        }
    }
}