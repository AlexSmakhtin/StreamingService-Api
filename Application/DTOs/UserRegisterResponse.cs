using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record UserRegisterResponse
{
    [Required, Length(2, 100)] public string Name { get; init; } = null!;
    [EmailAddress, Required] public string Email { get; init; } = null!;
    [Required] public string UserName { get; init; } = null!;
    [Required] public string JwtToken { get; init; } = null!;
    [Required] public Guid UserId { get; init; }
    [Required] public string Role { get; init; } = null!;
}