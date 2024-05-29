using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class TrackAddRequest
{
    [MinLength(2)] public string TrackName { get; init; } = null!;
    [FileExtensions(Extensions = ".mp3")] public IFormFile TrackFile { get; init; } = null!;
}