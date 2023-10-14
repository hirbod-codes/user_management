namespace user_management.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Privilege : IEquatable<Privilege>
{
    [BsonElement(NAME)]
    [BsonRequired]
    public string Name { get; set; } = null!;
    public const string NAME = "name";

    [BsonElement(VALUE)]
    [BsonRequired]
    public object? Value { get; set; }
    public const string VALUE = "value";

    public bool Equals(Privilege? other) => other != null && Name == other.Name && Object.Equals(Value, other.Value);

    public override bool Equals(object? obj) => obj != null && Equals(obj as Privilege);

    public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
}