namespace user_management.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using user_management.Validation.Attributes;

public class AllUpdaters
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
}