namespace user_management.Models;

using MongoDB.Bson.Serialization.Attributes;

public class Field
{
    [BsonElement(NAME)]
    [BsonRequired]
    public string Name { get; set; } = null!;
    public const string NAME = "name";

    [BsonElement(IS_PERMITTED)]
    [BsonRequired]
    public bool IsPermitted { get; set; }
    public const string IS_PERMITTED = "is_permitted";
}