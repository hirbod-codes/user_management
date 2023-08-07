namespace user_management.Dtos.User;

using user_management.Models;
using user_management.Validation.Attributes;

public class UpdaterRetrieveDto
{
    [ObjectId]
    public string? AuthorId { get; set; }
    [UpdaterAuthor]
    public string? Author { get; set; }
    public bool? IsPermitted { get; set; }
    [UpdaterFields]
    public Field[]? Fields { get; set; } = null!;
}