using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class TracksWithPagesCountGetResponse
{
    [Required]  public List<TrackGetResponseToShow> Tracks { get; set; } = [];
    [Required]  public int TotalPages { get; init; }

}