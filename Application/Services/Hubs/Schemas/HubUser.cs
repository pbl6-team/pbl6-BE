namespace PBL6.Application.Hubs.Schemas
{
    public class HubUser
    {
        public Guid UserId { get; set; }

        public HashSet<string> ConnectionIds { get; set; } = new();
    }
}
