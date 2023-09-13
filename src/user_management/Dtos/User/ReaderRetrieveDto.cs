namespace user_management.Dtos.User;

using user_management.Models;
using user_management.Validation.Attributes;

public class ReaderRetrieveDto
{
    [ObjectId]
    public string? AuthorId { get; set; }
    [ReaderAuthor]
    public string? Author { get; set; }
    public bool? IsPermitted { get; set; }
    [ReaderFields]
    public Field[]? Fields { get; set; } = null!;
}