using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class GetSubscriptionResponse
{
    [Required] public DateTime ExpireDate { get; set; }
    [Required] public string Name { get; set; } = null!;
}