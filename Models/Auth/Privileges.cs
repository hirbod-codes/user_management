namespace user_management.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Privilege
{
    [BsonElement(NAME)]
    [BsonRequired]
    public string Name { get; set; } = null!;
    public const string NAME = "name";

    [BsonElement(VALUE)]
    [BsonRequired]
    public dynamic? Value { get; set; }
    public const string VALUE = "value";
}