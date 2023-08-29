namespace user_management.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using user_management.Validation.Attributes;

public class AllUpdaters : IEquatable<AllUpdaters>
{
    [BsonElement(ARE_PERMITTED)]
    [BsonRequired]
    public bool ArePermitted { get; set; }
    public const string ARE_PERMITTED = "are_permitted";

    [BsonElement(FIELDS)]
    [BsonRequired]
    [UpdaterFields]
    public Field[] Fields { get; set; } = new Field[] { };
    public const string FIELDS = "fields";

    public bool Equals(AllUpdaters? other)
    {
        if (other == null || ArePermitted != other.ArePermitted) return false;

        for (int i = 0; i < Fields.Length; i++)
            if (!Object.Equals(Fields[i], other.Fields[i])) return false;

        return true;
    }

    public override bool Equals(object? obj) => obj != null && Equals((AllUpdaters)obj);
    public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
}