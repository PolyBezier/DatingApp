namespace API.DTOs;

public class LikeDto
{
    public int Id { get; set; }

    public required string UserName { get; set; }

    public int Age { get; set; }

    public required string KnownAs { get; set; }

    public string? PhotoUrl { get; set; }

    public required string City { get; set; }
}
