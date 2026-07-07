using System.ComponentModel.DataAnnotations;

namespace OffersService.DTOs;

public class RetailerCreateDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(2, MinimumLength = 2)]
    public string Country { get; set; } = string.Empty;
}
