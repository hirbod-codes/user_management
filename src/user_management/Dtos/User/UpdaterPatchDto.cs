namespace user_management.Dtos.User;

using user_management.Models;
using user_management.Validation.Attributes;

public class UpdaterPatchDto
{
    [ObjectId]
    public string AuthorId { get; set; } = null!;
    [UpdaterAuthor]
    public string Author { get; set; } = null!;
    public bool IsPermitted { get; set; }
    [UpdaterFields]
    public Field[] Fields { get; set; } = new Field[] { };
}