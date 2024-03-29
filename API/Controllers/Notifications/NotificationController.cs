using Microsoft.AspNetCore.Mvc;
using PBL6.Api.Controllers;
using PBL6.API.Filters;
using PBL6.Application.Contract.Notifications.Dtos;
using PBL6.Application.Services;
using Microsoft.AspNetCore.Http;

namespace PBL6.API.Controllers.Notifications
{
    public class NotificationController : BaseApiController
    {
        public INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// get notifications
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <response code="200">Returns notifications</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="404">If the notifications is not found</response>
        /// <response code="403">If the user is not authorized</response>
        /// <response code="500">If there was an internal server error</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<NotificationDto>))]
        [HttpGet]
        [AuthorizeFilter]
        public async Task<IActionResult> GetNotifications([FromQuery] SearchDto input)
        {
            var notifications = await _notificationService.SearchAsync(input);
            return Ok(notifications);
        }

        /// <summary>
        /// read notification
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="200">Returns notification</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="404">If the notification is not found</response>
        /// <response code="403">If the user is not authorized</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPut("{id}")]
        [AuthorizeFilter]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> ReadNotification(Guid id)
        {
            await _notificationService.ReadAsync(id);
            return NoContent();
        }

        /// <summary>
        /// delete notification
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        /// <response code="204">Delete success</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="404">If the notification is not found</response>
        /// <response code="403">If the user is not authorized</response>
        /// <response code="500">If there was an internal server error</response>\
        [HttpDelete()]
        [AuthorizeFilter]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> DeleteNotification(List<Guid> ids)
        {
            await _notificationService.DeleteAsync(ids);
            return NoContent();
        }

        /// <summary>
        /// count unread notification
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <response code="200">Returns notification</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="404">If the notification is not found</response>
        /// <response code="403">If the user is not authorized</response>
        /// <response code="500">If there was an internal server error</response>\
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        [HttpGet("count")]
        [AuthorizeFilter]
        public async Task<IActionResult> CountUnreadNotification([FromQuery] short? type)
        {
            var count = await _notificationService.CountUnreadNotification(type);
            return Ok(count);
        }
    }
}
