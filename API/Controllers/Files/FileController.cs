using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PBL6.API.Filters;
using PBL6.Application.Contract.Chats.Dtos;
using PBL6.Application.Contract.Common;

namespace PBL6.API.Controllers.Files
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FileController(IFileService fileService)
        {
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SendFileInfoDto))]
        [HttpPost]
        [AuthorizeFilter]
        public async Task<IActionResult> UploadFile([FromForm] List<IFormFile> files)
        {
            var fileInfos = new List<SendFileInfoDto>();
            foreach (var file in files)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var  url = await _fileService.UploadFileGetUrlAsync(fileName, file.OpenReadStream(), file.ContentType);

                var fileInfo = new SendFileInfoDto
                {
                    Name = file.FileName,
                    Type = file.ContentType,
                    Url = url
                };

                fileInfos.Add(fileInfo);
            }

            return Ok(fileInfos);
        }
    }
}
