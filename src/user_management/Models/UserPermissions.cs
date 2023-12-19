namespace user_management.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using user_management.Services.Client;

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

    public static async Task<UserPermissions> GetDefault(string authorId, IClientRepository clientRepository)
    {
        IEnumerable<Reader> readers = new Reader[] { new() { Author = Reader.USER, AuthorId = authorId, IsPermitted = true, Fields = User.GetReadableFields().ToArray() } };
        IEnumerable<Updater> updaters = new Updater[] { new() { Author = Updater.USER, AuthorId = authorId, IsPermitted = true, Fields = User.GetUpdatableFields().ToArray() } };
        IEnumerable<Deleter> deleters = new Deleter[] { new() { Author = Deleter.USER, AuthorId = authorId, IsPermitted = true } };

        IEnumerable<Client> firstPartyClients = await clientRepository.RetrieveFirstPartyClients();

        for (int i = 0; i < firstPartyClients.Count(); i++)
        {
            readers = readers.Append(new() { Author = Reader.USER, AuthorId = authorId, IsPermitted = true, Fields = User.GetReadableFields().ToArray() });
            updaters = updaters.Append(new() { Author = Reader.USER, AuthorId = authorId, IsPermitted = true, Fields = User.GetUpdatableFields().ToArray() });
            deleters = deleters.Append(new() { Author = Reader.USER, AuthorId = authorId, IsPermitted = true });
        }

        UserPermissions userPermissions = new()
        {
            AllReaders = new AllReaders() { ArePermitted = false },
            AllUpdaters = new AllUpdaters() { ArePermitted = false },
            Readers = readers.ToArray(),
            Updaters = updaters.ToArray(),
            Deleters = deleters.ToArray()
        };

        return userPermissions;
    }

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
