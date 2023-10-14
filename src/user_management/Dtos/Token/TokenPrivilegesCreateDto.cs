namespace user_management.Dtos.Token;

using user_management.Models;
using user_management.Validation.Attributes;

public class TokenPrivilegesCreateDto
{
    [PrivilegesValidation]
    public Privilege[] Privileges { get; set; } = new Privilege[] { };
    [ReaderFields]
    public Field[] ReadsFields { get; set; } = new Field[] { };
    [UpdaterFields]
    public Field[] UpdatesFields { get; set; } = new Field[] { };
    public bool DeletesUser { get; set; } = false;
}