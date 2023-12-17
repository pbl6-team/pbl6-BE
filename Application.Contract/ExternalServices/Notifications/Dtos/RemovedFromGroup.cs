namespace PBL6.Application.Contract.ExternalServices.Notifications.Dtos
{
    public class RemovedFromGroup
    {
        public Guid GroupId { get; set; }

        public string GroupName { get; set; }

        public Guid RemoverId { get; set; }

        public string RemoverName { get; set; }

        public string RemoverAvatar { get; set; }
    }
}