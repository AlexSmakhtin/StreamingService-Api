using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class PlaylistsWithCountOfPagesResponse
{
    [Required] public int TotalPages { get; set; }
    [Required] public List<PlaylistResponse> Playlists { get; set; } = [];
}