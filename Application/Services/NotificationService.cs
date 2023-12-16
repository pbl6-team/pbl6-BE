using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using OneSignalApi.Api;
using OneSignalApi.Client;
using PBL6.Common.Enum;
using PBL6.Domain.Data;
using PBL6.Domain.Models.Users;

namespace Application.Services
{
    public class NotificationService: INotificationService
    {
        private readonly DefaultApi _oneSignalApi;
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IConfiguration _config;

        public NotificationService(
            IUnitOfWork unitOfWork,
            IConfiguration config
        )
        {
            _unitOfWork = unitOfWork;
            _config = config;

            var appConfig = new Configuration
            {
                BasePath = _config["OneSignal:BasePath"],
                AccessToken = _config["OneSignal:ApiKey"]
            };
            _oneSignalApi = new DefaultApi(appConfig);
        }

        public async Task<string> SendNotificationAsync(
            string content,
            List<Guid> userIds = null,
            string title = "Fira",
            string subtitle = "Fira",
            short type = (short)NOTIFICATION_TYPE.GENERAL,
            Dictionary<string, string> data = null,
            DateTime? sendAfter = null,
            string icon = null,
            string url = null
          )
        {
            try
            {
                data ??= new Dictionary<string, string>();
                var notificationSignalR = new OneSignalApi.Model.Notification(appId: _config["OneSignal:AppId"])
                {
                    IncludeExternalUserIds = userIds?.Select(x => x.ToString()).ToList(),
                    IncludedSegments = userIds.IsNullOrEmpty() ? new List<string> { "All" } : null,
                    Headings = new OneSignalApi.Model.StringMap(en: title),
                    Contents = new OneSignalApi.Model.StringMap(en: content),
                    Subtitle = new OneSignalApi.Model.StringMap(en: subtitle),
                    Data = data,
                    WebUrl = url ?? _config["BaseUrl"],
                    SendAfter = sendAfter
                };

                if(icon != null && _config["OneSignal:SmallIcon"] != null)
                {
                    notificationSignalR.ChromeWebIcon = icon ?? _config["OneSignal:SmallIcon"];
                    notificationSignalR.ChromeWebImage = icon ?? _config["OneSignal:SmallIcon"];
                    notificationSignalR.ChromeWebBadge = icon ?? _config["OneSignal:SmallIcon"];
                    notificationSignalR.AdmSmallIcon = icon ?? _config["OneSignal:SmallIcon"];
                    notificationSignalR.SmallIcon = icon ?? _config["OneSignal:SmallIcon"];
                }

                if (!string.IsNullOrEmpty(_config["OneSignal:IosSound"]) && !string.IsNullOrEmpty(_config["OneSignal:AndroidSound"]))
                {
                    notificationSignalR.AndroidChannelId = _config["OneSignal:AndroidChannelId"];
                    notificationSignalR.AndroidSound = _config["OneSignal:AndroidSound"];
                    notificationSignalR.IosSound = _config["OneSignal:IosSound"];
                }
                
                var result = await _oneSignalApi.CreateNotificationAsync(notificationSignalR);
                if (result.Id != null)
                {
                    return result.Id;
                }
            }
            catch (ApiException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.ErrorCode);
                Console.WriteLine(e.ErrorContent);
                Console.WriteLine(e.StackTrace);
            }

            return null;
        }
    
        public async Task SendNotificationAsync(Guid notificationId)
        {
            var notification = await _unitOfWork.Notifications.GetUnsentNotificationById(notificationId);
            if (notification is not null)
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(notification.Data);
                var url = data.TryGetValue("Url", out var u) ? u : null;
                url ??= $"{_config["BaseUrl"]}/notification/{notification.Id}";
                data.Remove("notificationId");
                data.TryAdd("notificationId", notification.Id.ToString());
                var result = await SendNotificationAsync(
                    notification.Content,
                    notification.UserNotifications.Select(x => x.UserId).ToList(),
                    notification.Title,
                    notification.Title,
                    notification.Type,
                    data,
                    notification.TimeToSend < DateTime.Now ? null : notification.TimeToSend,
                    data.TryGetValue("icon", out var icon) ? icon : null,
                    url
                );
                if (!result.IsNullOrEmpty())
                {
                    notification.Status = (short)NOTIFICATION_STATUS.SENT;
                    notification.UserNotifications.ToList().ForEach(x => x.PushId = Guid.Parse(result));
                    notification.Data = JsonConvert.SerializeObject(data);
                    await _unitOfWork.Notifications.UpdateAsync(notification);
                    await _unitOfWork.SaveChangeAsync();
                }
                else
                {
                    notification.Status = (short)NOTIFICATION_STATUS.FAILED;
                    await _unitOfWork.Notifications.UpdateAsync(notification);
                    await _unitOfWork.SaveChangeAsync();
                }
            }
        }
    }
}
