using Microsoft.AspNetCore.Mvc;
using PBL6.API.Filters;
using PBL6.Application.Contract.Chats;
using PBL6.Application.Contract.Chats.Dtos;
using PBL6.Application.Contract.Common;

namespace PBL6.API.Controllers.Files
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly IChatService _chatService;

        public FileController(IFileService fileService, IChatService chatService)
        {
            _chatService = chatService;
            _fileService = fileService;
        }

        /// <summary>
        /// upload files
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        /// <response code="200">Returns file info</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="500">If there was an internal server error</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<FileInfoDto>))]
        [HttpPost]
        [AuthorizeFilter]
        public async Task<IActionResult> UploadFile([FromForm] List<IFormFile> files)
        {
            var fileInfos = new List<FileInfoDto>();
            foreach (var file in files)
            {
                var extension = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid()}{extension}";
                if (file.Length <= 0) continue;
                var url = await _fileService.UploadFileGetUrlAsync(
                    fileName,
                    file.OpenReadStream(),
                    file.ContentType
                );

                var fileInfo = await _chatService.SaveFileAsync(new SendFileInfoDto
                {
                    Name = file.FileName,
                    Type = file.ContentType,
                    Url = url
                });

                fileInfos.Add(fileInfo);
            }

            return Ok(fileInfos);
        }

        /// <summary>
        /// Get files
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <response code="200">Returns file info</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="500">If there was an internal server error</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<FileInfoDto>))]
        [HttpGet]
        [AuthorizeFilter]
        public async Task<IActionResult> GetFiles([FromQuery] GetFileDto input)
        {
            var files = await _chatService.GetFilesAsync(input);
            return Ok(files);
        }
    }
}
