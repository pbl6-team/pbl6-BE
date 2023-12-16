namespace PBL6.Application.Contract.Notifications.Dtos
{
    public class InvitedToNewGroup
    {
        public Guid GroupId { get; set; }

        public string GroupName { get; set; }

        public Guid InviterId { get; set; }

        public string InviterName { get; set; }

        public string InviterAvatar { get; set; }
    }
}