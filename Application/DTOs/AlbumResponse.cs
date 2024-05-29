using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class AlbumResponse
{
    [Required] public Guid Id { get; init; }
    [Required] public Guid MusicianId { get; init; }
    [Required] public string Name { get; init; } = null!;
    [Required] public string AuthorName { get; init; } = null!;
    [Required] public List<TrackGetResponseToShow> Tracks { get; init; } = null!;

    [Required] public int CountOfTracks { get; init; }
}