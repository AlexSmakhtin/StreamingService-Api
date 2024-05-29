using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class SubscriptionToBuyResponse
{
    [Required] public Guid Id { get; init; }
    [Required] public int Duration { get; init; }
    [Required] public string Name { get; init; } = null!;
    [Required] public decimal Cost { get; init; }
}