using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class MusicianResponse
{
    [Required] public Guid Id { get; init; }

    [Required] public string Name { get; init; } = null!;

    [Required] public int TracksCount { get; init; }

    [Required] public int AlbumsCount { get; init; }

    [Required] public long Listening { get; init; }
}