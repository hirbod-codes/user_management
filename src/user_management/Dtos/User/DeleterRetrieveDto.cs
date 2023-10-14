using user_management.Validation.Attributes;

namespace user_management.Dtos.User;

public class DeleterRetrieveDto
{
    [ObjectId]
    public string? AuthorId { get; set; }
    [DeleterAuthor]
    public string? Author { get; set; }
    public bool? IsPermitted { get; set; }
}