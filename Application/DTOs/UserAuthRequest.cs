using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record UserAuthRequest
{
    [EmailAddress, Required] public required string Email { get; init; }
    [Required] public required string Password { get; init; }
}