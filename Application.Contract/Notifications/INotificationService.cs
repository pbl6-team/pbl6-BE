namespace Application.Services
{
    public interface INotificationService
    {
        Task<string> SendNotificationAsync(
            string message,
            List<Guid> userIds,
            string title = "Fira",
            string subtitle = "Fira",
            short type = 1,
            Dictionary<string, string> data = null,
            DateTime? sendAfter = null,
            string icon = null,
            string url = null
        );
        Task SendNotificationAsync(Guid notificationId);
    }
}
