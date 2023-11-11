using System.ComponentModel.DataAnnotations;

namespace PBL6.Application.Contract.Channels.Dtos;

public class UpdateChannelDto
{
    [StringLength(50)]
    [Required]
    public string Name { get; set; }

    public string Description { get; set; }

    public Guid? CategoryId { get; set; }
}
