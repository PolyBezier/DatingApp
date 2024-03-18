using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class RegisterDto
{
    [Required]
    public string Username { get; set; } = null!;

    [Required]
    public string KnownAs { get; set; } = null!;

    [Required]
    public string Gender { get; set; } = null!;

    [Required]
    public DateOnly? DateOfBirth { get; set; }

    [Required]
    public string City { get; set; } = null!;

    [Required]
    public string Country { get; set; } = null!;

    [Required]
    [StringLength(8, MinimumLength = 1)]
    public string Password { get; set; } = null!;
}
