using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using PBL6.Application.Contract.Common;

namespace PBL6.Application.Services
{
    public class FileService : IFileService
    {
        private string BucketName => _config["Minio:BucketName"];
        private readonly string _baseUrl;

        private readonly IMinioClient _minioClient;
        private readonly IConfiguration _config;

        public FileService(IConfiguration config)
        {
            _config = config;
            _baseUrl =
                $"{(_config["Minio:UseSSL"] == "False" ? "http://" : "https://")}{_config["Minio:EndPoint"]}/{BucketName}/";
            _minioClient = new MinioClient()
                .WithEndpoint(_config["Minio:EndPoint"])
                .WithCredentials(_config["Minio:AccessKey"], _config["Minio:SecretKey"])
                .WithSSL(_config["Minio:UseSSL"] == "True")
                .Build();
        }

        public async Task UploadFileAsync(string fileName, Stream stream, string contentType = "")
        {
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(BucketName)
                .WithObject(fileName)
                .WithObjectSize(-1)
                .WithStreamData(stream)
                .WithContentType(contentType);

            await _minioClient.PutObjectAsync(putObjectArgs);
        }

        public async Task<(Stream, string)> DownloadFileAsync(string fileName)
        {
            var fileStream = new MemoryStream();
            var getObjectArgs = new GetObjectArgs()
                .WithBucket(BucketName)
                .WithObject(fileName)
                .WithCallbackStream(
                    (stream) =>
                    {
                        stream.CopyTo(fileStream);
                    }
                );

            var file = await _minioClient.GetObjectAsync(getObjectArgs);
            fileStream.Position = 0;
            return (fileStream, file.ContentType);
        }

        public async Task<bool> FileExistsAsync(string fileName)
        {
            try
            {
                var statObjectArgs = new StatObjectArgs()
                    .WithBucket(BucketName)
                    .WithObject(fileName);
                await _minioClient.StatObjectAsync(statObjectArgs);
                return true;
            }
            catch (MinioException)
            {
                return false;
            }
        }

        public async Task DeleteFileAsync(List<string> fileNames)
        {
            var removeObjectArgs = new RemoveObjectsArgs()
                .WithBucket(BucketName)
                .WithObjects(fileNames);
            await _minioClient.RemoveObjectsAsync(removeObjectArgs);
        }

        public async Task CopyFileAsync(string fileName, string newFileName)
        {
            var src = new CopySourceObjectArgs().WithBucket(BucketName).WithObject(fileName);
            var args = new CopyObjectArgs()
                .WithBucket(BucketName)
                .WithObject(newFileName)
                .WithCopyObjectSource(src);

            await _minioClient.CopyObjectAsync(args);
        }

        public async Task DeleteFileUrlAsync(List<string> filePath)
        {
            try
            {
                if (filePath != null && filePath.Count > 0)
                {
                    filePath = filePath
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Select(x => x.Replace(_baseUrl, ""))
                        .Distinct()
                        .ToList();
                    var removeObjectArgs = new RemoveObjectsArgs()
                        .WithBucket(BucketName)
                        .WithObjects(filePath);
                    await _minioClient.RemoveObjectsAsync(removeObjectArgs);
                }
            }
            catch
            {
                return;
            }
        }

        public async Task<string> UploadFileGetUrlAsync(
            string fileName,
            Stream stream,
            string contentType = ""
        )
        {
            try
            {
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(BucketName)
                    .WithObject(fileName)
                    .WithObjectSize(-1)
                    .WithStreamData(stream)
                    .WithContentType(contentType);
                await _minioClient.PutObjectAsync(putObjectArgs);

                return $"{_baseUrl}{fileName}";
            }
            catch (MinioException ex)
            {
                throw ex;
            }
        }

        public string GetBaseUrl()
        {
            return _baseUrl;
        }
    }
}
