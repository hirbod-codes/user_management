namespace user_management.Dtos.User;

using user_management.Models;

public class ReaderRetrieveDto
{
    public string? AuthorId { get; set; }
    public string? Author { get; set; }
    public bool? IsPermitted { get; set; }
    public Field[]? Fields { get; set; } = null!;
}