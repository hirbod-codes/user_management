namespace user_management.Models;

public class FakeUserOptions
{
    /// <summary>
    /// If false includes all privileges in each user.
    /// </summary>
    public bool RandomPrivileges = true;
    /// <summary>
    /// Each user will randomly authorizes a client which randomly demands privileges, read fields, etc...
    /// </summary>
    public bool RandomClients = true;
    public bool GiveUserPrivilegesToRandomUsers = true;
    public bool GiveUserPrivilegesToItSelf = true;
}