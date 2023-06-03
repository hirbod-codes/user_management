namespace user_management.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class AllReaders
{
    [BsonElement(ARE_PERMITTED)]
    [BsonRequired]
    public bool ArePermitted { get; set; }
    public const string ARE_PERMITTED = "are_permitted";

    [BsonElement(FIELDS)]
    [BsonRequired]
    public Field[] Fields { get; set; } = new Field[] { };
    public const string FIELDS = "fields";
}