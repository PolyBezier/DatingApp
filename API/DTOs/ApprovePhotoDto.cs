namespace API.DTOs;

public class ApprovePhotoDto
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public bool Approve { get; set; }
}
