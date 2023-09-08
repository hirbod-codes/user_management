namespace user_management.Models;

using MongoDB.Bson.Serialization.Attributes;

public class Field : IEquatable<Field>
{
    [BsonElement(NAME)]
    [BsonRequired]
    public string Name { get; set; } = null!;
    public const string NAME = "name";

    [BsonElement(IS_PERMITTED)]
    [BsonRequired]
    public bool IsPermitted { get; set; }
    public const string IS_PERMITTED = "is_permitted";

    public bool Equals(Field? other) =>
        other != null &&
        Name == other.Name &&
        IsPermitted == other.IsPermitted;

    public override bool Equals(object? obj) => obj != null && Equals(obj as Field);
    public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
}