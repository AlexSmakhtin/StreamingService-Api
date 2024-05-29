using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class UpdateUserRequest
{
    [Required] public string Name { get; set; } = null!;
    [Required] public string Email { get; set; } = null!;
}