using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class ChangePasswordRequest
{
    [Required] public string OldPassword { get; set; } = null!;
    [Required] public string NewPassword { get; set; } = null!;
}