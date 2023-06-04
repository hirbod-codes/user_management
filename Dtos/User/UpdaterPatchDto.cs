namespace user_management.Dtos.User;

using user_management.Models;

public class UpdaterPatchDto
{
    public string AuthorId { get; set; } = null!;
    public string Author { get; set; } = null!;
    public bool IsPermitted { get; set; }
    public Field[] Fields { get; set; } = new Field[] { };
}