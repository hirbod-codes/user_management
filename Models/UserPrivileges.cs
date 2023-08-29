namespace user_management.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

[BsonIgnoreExtraElements]
public class UserPrivileges : IEquatable<UserPrivileges>
{
    [BsonElement(READERS)]
    [BsonRequired]
    public Reader[] Readers { get; set; } = new Reader[] { };
    public const string READERS = "readers";

    [BsonElement(ALL_READERS)]
    [BsonRequired]
    public AllReaders? AllReaders { get; set; }
    public const string ALL_READERS = "all_readers";

    [BsonElement(UPDATERS)]
    [BsonRequired]
    public Updater[] Updaters { get; set; } = new Updater[] { };
    public const string UPDATERS = "updaters";

    [BsonElement(ALL_UPDATERS)]
    [BsonRequired]
    public AllUpdaters? AllUpdaters { get; set; }
    public const string ALL_UPDATERS = "all_updaters";

    [BsonElement(DELETERS)]
    [BsonRequired]
    public Deleter[] Deleters { get; set; } = new Deleter[] { };
    public const string DELETERS = "deleters";

    public bool Equals(UserPrivileges? other)
    {
        if (other == null) return false;

        for (int i = 0; i < Readers.Length; i++)
            if (!Object.Equals(Readers[i], other.Readers[i])) return false;

        for (int i = 0; i < Updaters.Length; i++)
            if (!Object.Equals(Updaters[i], other.Updaters[i])) return false;

        for (int i = 0; i < Deleters.Length; i++)
            if (!Object.Equals(Deleters[i], other.Deleters[i])) return false;

        if (!Object.Equals(AllReaders, other.AllReaders)) return false;
        if (!Object.Equals(AllUpdaters, other.AllUpdaters)) return false;

        return true;
    }

    public override bool Equals(object? obj) => obj != null && Equals((UserPrivileges)obj);
    public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
}