using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using PBL6.Application.Contract.Notifications.Dtos;

namespace PBL6.Application.Services
{
    public interface INotificationService
    {
        Task<List<NotificationDto>> GetAllAsync(SearchDto dto);
        Task ReadAsync(Guid id);
        Task DeleteAsync(Guid id);
    }

    public class NotificationService : BaseService, INotificationService
    {
        private readonly string _className;

        public NotificationService(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _className = nameof(NotificationService);
        }

        static string GetActualAsyncMethodName([CallerMemberName] string name = null) => name;

        public async Task<List<NotificationDto>> GetAllAsync(SearchDto dto)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var currentUserId = Guid.Parse(_currentUser.UserId);
            var notifications = await _unitOfWork.Notifications.GetUserNotifications(
                currentUserId, dto.Offset, dto.Limit
            );
            var notificationDtos = _mapper.Map<List<NotificationDto>>(notifications);

            _logger.LogInformation("[{_className}][{method}] End", _className, method);
            return notificationDtos;
        }

        public async Task ReadAsync(Guid id)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var currentUserId = Guid.Parse(_currentUser.UserId);
            await _unitOfWork.Notifications.ReadUserNotification(currentUserId, id);

            _logger.LogInformation("[{_className}][{method}] End", _className, method);
        }

        public async Task DeleteAsync(Guid id)
        {
            var method = GetActualAsyncMethodName();
            _logger.LogInformation("[{_className}][{method}] Start", _className, method);
            var currentUserId = Guid.Parse(_currentUser.UserId);
            await _unitOfWork.Notifications.DeleteUserNotification(currentUserId, id);

            _logger.LogInformation("[{_className}][{method}] End", _className, method);
        }
    }
}
