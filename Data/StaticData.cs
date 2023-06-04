namespace user_management.Data;

using user_management.Models;

public static class StaticData
{
    public const string READ_ACCOUNT = "read_account";
    public const string READ_ACCOUNTS = "read_accounts";
    public const string UPDATE_ACCOUNT = "update_account";
    public const string UPDATE_ACCOUNTS = "update_accounts";
    public const string DELETE_ACCOUNT = "delete_account";
    public const string DELETE_ACCOUNTS = "delete_accounts";
    public const string REGISTER_CLIENT = "register_client";
    public const string READ_CLIENT = "read_client";
    public const string UPDATE_CLIENT = "update_client";
    public const string DELETE_CLIENT = "delete_client";
    public const string AUTHORIZE_CLIENT = "authorize_client";
    public static List<Privilege> Privileges = new() {
            // Accounts
            new Privilege()
            {
                Name = "read_account",
                Value = true
            },
            new Privilege()
            {
                Name = "read_accounts",
                Value = true
            },
            new Privilege()
            {
                Name = "update_account",
                Value = true
            },
            new Privilege()
            {
                Name = "update_accounts",
                Value = true
            },
            new Privilege()
            {
                Name = "delete_account",
                Value = true
            },
            new Privilege()
            {
                Name = "delete_accounts",
                Value = true
            },
            // Clients
            new Privilege()
            {
                Name = "register_client",
                Value = true
            },
            new Privilege()
            {
                Name = "read_client",
                Value = true
            },
            new Privilege()
            {
                Name = "update_client",
                Value = true
            },
            new Privilege()
            {
                Name = "delete_client",
                Value = true
            },
            new Privilege()
            {
                Name = "delete_clients",
                Value = true
            },
            new Privilege()
            {
                Name = "authorize_client",
                Value = true
            }
        };

    public static List<Privilege> GetDefaultUserPrivileges()
    {
        return Privileges;
    }
}