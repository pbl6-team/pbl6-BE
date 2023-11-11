using System.ComponentModel.DataAnnotations;

namespace PBL6.Application.Contract.Channels.Dtos
{
    public class CreateChannelDto
    {
        [StringLength(50)]
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public Guid WorkspaceId { get; set; }
    }
}