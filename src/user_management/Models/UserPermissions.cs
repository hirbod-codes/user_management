namespace user_management.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

[BsonIgnoreExtraElements]
public class UserPermissions : IEquatable<UserPermissions>
{
    [BsonElement(READERS)]
    [BsonRequired]
    public Reader[] Readers { get; set; } = Array.Empty<Reader>();
    public const string READERS = "readers";

    [BsonElement(ALL_READERS)]
    [BsonRequired]
    public AllReaders? AllReaders { get; set; }
    public const string ALL_READERS = "all_readers";

    [BsonElement(UPDATERS)]
    [BsonRequired]
    public Updater[] Updaters { get; set; } = Array.Empty<Updater>();
    public const string UPDATERS = "updaters";

    [BsonElement(ALL_UPDATERS)]
    [BsonRequired]
    public AllUpdaters? AllUpdaters { get; set; }
    public const string ALL_UPDATERS = "all_updaters";

    [BsonElement(DELETERS)]
    [BsonRequired]
    public Deleter[] Deleters { get; set; } = Array.Empty<Deleter>();
    public const string DELETERS = "deleters";

    public bool Equals(UserPermissions? other)
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

    public override bool Equals(object? obj) => obj != null && Equals(obj as UserPermissions);
    public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
}
