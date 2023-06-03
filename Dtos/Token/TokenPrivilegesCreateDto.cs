namespace user_management.Dtos.Token;

using user_management.Dtos.User;
using user_management.Models;

public class TokenPrivilegesCreateDto
{
    public Field[] ReadsFields { get; set; } = new Field[] { };
    public Field[] UpdatesFields { get; set; } = new Field[] { };
    public bool DeletesUser { get; set; } = false;
}