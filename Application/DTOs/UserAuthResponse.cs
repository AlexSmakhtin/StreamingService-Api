using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class UserAuthResponse
{
    [Required] public string JwtToken { get; init; } = null!;
    [Required] public string UserName { get; set; } = null!;
    [Required] public Guid UserId { get; init; }

    public string Role { get; init; } = null!;
}