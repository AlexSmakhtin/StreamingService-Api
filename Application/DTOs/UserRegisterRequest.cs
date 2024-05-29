using System.ComponentModel.DataAnnotations;
using Domain.Entities.Enums;

namespace Application.DTOs;

public record UserRegisterRequest
{
    [Required, Length(2, 100)] public string Name { get; init; } = null!;

    [Required, EmailAddress] public string Email { get; init; } = null!;
    
    [Required] public Roles Role { get; init; }

    [Required] public Statuses Status { get; init; }

    [Required] public DateTime Birthday { get; init; }

    [Required, RegularExpression("^(?=.*[A-Za-z])(?=.*\\d)(?=.*[!@#$%^&*()-_=+{};:,<.>]).{12,}$")]
    public string Password { get; init; } = null!;
}