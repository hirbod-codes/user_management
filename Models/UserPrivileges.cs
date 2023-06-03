namespace user_management.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

[BsonIgnoreExtraElements]
public class UserPrivileges
{
    [BsonElement(PRIVILEGES)]
    [BsonRequired]
    public Privilege[] Privileges { get; set; } = new Privilege[] { };
    public const string PRIVILEGES = "privileges";

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
}