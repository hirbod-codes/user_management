namespace user_management.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Reader : IEquatable<Reader>
{
    [BsonElement(AUTHOR_ID)]
    [BsonRequired]
    public ObjectId AuthorId { get; set; }
    public const string AUTHOR_ID = "author_id";

    [BsonElement(IS_PERMITTED)]
    [BsonRequired]
    public bool IsPermitted { get; set; }
    public const string IS_PERMITTED = "is_permitted";

    [BsonElement(FIELDS)]
    [BsonRequired]
    public Field[] Fields { get; set; } = null!;
    public const string FIELDS = "fields";

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

    public bool Equals(Reader? other)
    {
        if (other == null || Author != other.Author || AuthorId.ToString() != other.AuthorId.ToString() || IsPermitted != other.IsPermitted) return false;

        for (int i = 0; i < Fields.Length; i++)
            if (!Object.Equals(Fields[i], other.Fields[i])) return false;

        return true;
    }

    public override bool Equals(object? obj) => obj != null && Equals((Reader)obj);
    public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
}