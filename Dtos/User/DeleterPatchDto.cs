namespace user_management.Dtos.User;

public class DeleterPatchDto
{
    public string AuthorId { get; set; } = null!;
    public string Author { get; set; } = null!;
    public bool IsPermitted { get; set; }
}