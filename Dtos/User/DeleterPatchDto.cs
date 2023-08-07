using user_management.Validation.Attributes;

namespace user_management.Dtos.User;

public class DeleterPatchDto
{
    [ObjectId]
    public string AuthorId { get; set; } = null!;
    [DeleterAuthor]
    public string Author { get; set; } = null!;
    public bool IsPermitted { get; set; }
}