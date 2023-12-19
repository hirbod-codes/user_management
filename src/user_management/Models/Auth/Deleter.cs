namespace user_management.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Deleter : IEquatable<Deleter>
{
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement(AUTHOR_ID)]
    [BsonRequired]
    public string AuthorId { get; set; } = null!;
    public const string AUTHOR_ID = "author_id";

    [BsonElement(IS_PERMITTED)]
    [BsonRequired]
    public bool IsPermitted { get; set; }
    public const string IS_PERMITTED = "is_permitted";

    [BsonElement(AUTHOR)]
    [BsonRequired]
    public string Author
    {
        get
        {
            if (_author == null)
                throw new ArgumentNullException();
            return _author;
        }
        set
        {
            if (!ValidateAuthor(value))
                throw new ArgumentException();
            _author = value;
        }
    }
    private string? _author;
    public const string AUTHOR = "author";
    public const string USER = "user";
    public const string CLIENT = "client";

    public static bool ValidateAuthor(string value) => value == USER || value == CLIENT;

    public bool Equals(Deleter? other) =>
        other != null &&
        AuthorId.ToString() == other.AuthorId.ToString() &&
        IsPermitted == other.IsPermitted &&
        Author == other.Author;

    public override bool Equals(object? obj) => obj != null && Equals(obj as Deleter);
    public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
}
