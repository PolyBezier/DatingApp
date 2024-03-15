using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class RegisterDto
{
    [Required]
    public required string Username { get; set; }

    [Required]
    [StringLength(8, MinimumLength = 1)]
    public required string Password { get; set; }
}
