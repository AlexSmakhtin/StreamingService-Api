using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class PlaylistUpdateRequest
{
    [Required] public string Name { get; set; } = null!;
    [Required] public List<Guid> TrackIds { get; set; } = [];
}