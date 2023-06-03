namespace user_management.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Reader
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

    private bool ValidateAuthor(string value) => value == USER || value == CLIENT;
}