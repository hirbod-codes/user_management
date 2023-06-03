namespace user_management.Dtos.User;

public class DeleterRetrieveDto
{
    public string? AuthorId { get; set; }
    public string? Author { get; set; }
    public bool? IsPermitted { get; set; }
}