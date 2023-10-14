namespace user_management.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

[BsonIgnoreExtraElements]
public class TokenPrivileges : IEquatable<TokenPrivileges>
{
    [BsonElement(PRIVILEGES)]
    [BsonRequired]
    public Privilege[] Privileges { get; set; } = new Privilege[] { };
    public const string PRIVILEGES = "privileges";

    [BsonElement(READS)]
    public Field[] ReadsFields { get; set; } = new Field[] { };
    public const string READS = "reads";

    [BsonElement(UPDATES)]
    public Field[] UpdatesFields { get; set; } = new Field[] { };
    public const string UPDATES = "updates";

    [BsonElement(DELETES)]
    public bool DeletesUser { get; set; } = false;
    public const string DELETES = "deletes";

    public bool Equals(TokenPrivileges? other)
    {
        if (other == null) return false;

        for (int i = 0; i < Privileges.Length; i++)
            if (!Object.Equals(Privileges[i], other.Privileges[i])) return false;

        for (int i = 0; i < ReadsFields.Length; i++)
            if (!Object.Equals(ReadsFields[i], other.ReadsFields[i])) return false;

        for (int i = 0; i < UpdatesFields.Length; i++)
            if (!Object.Equals(UpdatesFields[i], other.UpdatesFields[i])) return false;

        if (DeletesUser != other.DeletesUser) return false;

        return true;
    }

    public override bool Equals(object? obj) => obj != null && Equals(obj as TokenPrivileges);
    public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
}