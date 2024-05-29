using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class PlaylistResponse
{
    [Required] public Guid Id { get; init; }
    [Required] public string Name { get; init; } = null!;
    [Required] public List<TrackGetResponseToShow> Tracks { get; init; } = null!;
    [Required] public int CountOfTracks { get; set; }
}