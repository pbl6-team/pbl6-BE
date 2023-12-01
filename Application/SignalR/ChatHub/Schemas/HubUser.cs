namespace PBL6.Application.SignalR.ChatHub.Schemas
{
    public class HubUser
    {
        public Guid UserId { get; set; }

        public HashSet<string> ConnectionIds { get; set; } = new();
    }
}
