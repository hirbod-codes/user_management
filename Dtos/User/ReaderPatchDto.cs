namespace user_management.Dtos.User;

using user_management.Models;
using user_management.Validation.Attributes;

public class ReaderPatchDto
{
    [ObjectId]
    public string AuthorId { get; set; } = null!;
    [ReaderAuthor]
    public string Author { get; set; } = null!;
    public bool IsPermitted { get; set; }
    [ReaderFields]
    public Field[] Fields { get; set; } = new Field[] { };
}