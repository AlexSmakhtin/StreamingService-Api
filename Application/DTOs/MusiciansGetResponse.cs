using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class MusiciansGetResponse
{
    [Required]  public List<MusicianResponse> Musicians { get; set; } = [];
    [Required]  public int TotalPages { get; init; }
}