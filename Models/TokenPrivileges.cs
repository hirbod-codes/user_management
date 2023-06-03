namespace user_management.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

[BsonIgnoreExtraElements]
public class TokenPrivileges
{
    [BsonElement(READS)]
    public Field[] ReadsFields { get; set; } = new Field[] { };
    public const string READS = "reads";

    [BsonElement(UPDATES)]
    public Field[] UpdatesFields { get; set; } = new Field[] { };
    public const string UPDATES = "updates";

    [BsonElement(DELETES)]
    public bool DeletesUser { get; set; } = false;
    public const string DELETES = "deletes";
}