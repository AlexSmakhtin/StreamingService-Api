using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class AlbumsWithPagesCountGetResponse
{
    [Required] public List<AlbumResponse> Albums { get; set; } = [];
    [Required] public int TotalPages { get; init; }
}