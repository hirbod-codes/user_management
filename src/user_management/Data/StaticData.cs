namespace user_management.Data;

using System;
using user_management.Models;

public static class StaticData
{
    public static bool AreValid(IEnumerable<Privilege> privileges)
    {
        if (privileges.Count() > Privileges.Count) return false;

        foreach (Privilege privilege in privileges)
            if (Privileges.FirstOrDefault<Privilege?>(p => p != null && p.Name == privilege.Name, null) == null)
                return false;
        return true;
    }

    public static bool IsEntityPrivileged(IEnumerable<Privilege> referenceEntityPrivileges, IEnumerable<Privilege> underValidationEntityPrivileges)
    {
        if (underValidationEntityPrivileges.Count() > referenceEntityPrivileges.Count()) return false;

        for (int i = 0; i < referenceEntityPrivileges.Count(); i++)
        {
            Privilege? p = underValidationEntityPrivileges.FirstOrDefault<Privilege?>(p => p != null && p.Name == referenceEntityPrivileges.ElementAt(i).Name, null);
            if (p == null || p.Value == null || (bool)(p.Value) != true) return false;
        }
        return true;
    }

    public const string READ_ACCOUNT = "read_account";
    public const string READ_ACCOUNTS = "read_accounts";
    public const string UPDATE_ACCOUNT = "update_account";
    public const string UPDATE_ACCOUNTS = "update_accounts";
    public const string DELETE_ACCOUNT = "delete_account";
    public const string DELETE_ACCOUNTS = "delete_accounts";
    public const string REGISTER_CLIENT = "register_client";
    public const string READ_CLIENT = "read_client";
    public const string READ_CLIENTS = "read_clients";
    public const string UPDATE_CLIENT = "update_client";
    public const string DELETE_CLIENT = "delete_client";
    public const string DELETE_CLIENTS = "delete_clients";
    public const string AUTHORIZE_CLIENT = "authorize_client";
    public const string UPDATE_READERS = "update_readers";
    public const string UPDATE_ALL_READERS = "update_all_readers";
    public const string UPDATE_UPDATERS = "update_updaters";
    public const string UPDATE_ALL_UPDATERS = "update_all_updaters";
    public const string UPDATE_DELETERS = "update_deleters";

    public static List<Privilege> Privileges = new() {
            // Accounts
            new Privilege()
            {
                Name = READ_ACCOUNT,
                Value = true
            },
            new Privilege()
            {
                Name = READ_ACCOUNTS,
                Value = true
            },
            new Privilege()
            {
                Name = UPDATE_ACCOUNT,
                Value = true
            },
            new Privilege()
            {
                Name = UPDATE_ACCOUNTS,
                Value = true
            },
            new Privilege()
            {
                Name = DELETE_ACCOUNT,
                Value = true
            },
            new Privilege()
            {
                Name = DELETE_ACCOUNTS,
                Value = true
            },
            // Clients
            new Privilege()
            {
                Name = REGISTER_CLIENT,
                Value = true
            },
            new Privilege()
            {
                Name = READ_CLIENT,
                Value = true
            },
            new Privilege()
            {
                Name = READ_CLIENTS,
                Value = true
            },
            new Privilege()
            {
                Name = UPDATE_CLIENT,
                Value = true
            },
            new Privilege()
            {
                Name = DELETE_CLIENT,
                Value = true
            },
            new Privilege()
            {
                Name = DELETE_CLIENTS,
                Value = true
            },
            new Privilege()
            {
                Name = AUTHORIZE_CLIENT,
                Value = true
            },
            // Privileges
            new Privilege()
            {
                Name = UPDATE_READERS,
                Value = true
            },
            new Privilege()
            {
                Name = UPDATE_ALL_READERS,
                Value = true
            },
            new Privilege()
            {
                Name = UPDATE_UPDATERS,
                Value = true
            },
            new Privilege()
            {
                Name = UPDATE_ALL_UPDATERS,
                Value = true
            },
            new Privilege()
            {
                Name = UPDATE_DELETERS,
                Value = true
            },
        };

    public static List<Privilege> GetDefaultUserPrivileges() => Privileges;
    public static List<Privilege> GetAllPrivileges() => Privileges;
}
