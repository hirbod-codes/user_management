namespace user_management.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;

[EntityTypeConfiguration(typeof(Client))]
[BsonIgnoreExtraElements]
public class Client : IEntityTypeConfiguration<Client>
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonRequired]
    [Key]
    public string Id { get; set; } = null!;

    [BsonElement(IS_FIRST_PARTY)]
    [BsonRequired]
    public bool IsFirstParty { get; set; } = false;
    public const string IS_FIRST_PARTY = "is_first_party";

    [BsonElement(SECRET)]
    [BsonRequired]
    public string Secret { get; set; } = null!;
    public const string SECRET = "secret";

    [BsonElement(REDIRECT_URL)]
    [BsonRequired]
    public string RedirectUrl { get; set; } = null!;
    public const string REDIRECT_URL = "redirect_url";

    [BsonElement(TOKENS_EXPOSED_AT)]
    [BsonRequired]
    public DateTime? TokensExposedAt { get; set; } = null;
    public const string TOKENS_EXPOSED_AT = "tokens_exposed_at";

    [BsonElement(EXPOSED_COUNT)]
    [BsonRequired]
    public int ExposedCount { get; set; } = 0;
    public const string EXPOSED_COUNT = "exposed_count";

    [BsonElement(CREATED_AT)]
    [BsonRequired]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public const string CREATED_AT = "created_at";

    [BsonElement(UPDATED_AT)]
    [BsonRequired]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public const string UPDATED_AT = "updated_at";

    public void Configure(EntityTypeBuilder<Client> builder) => builder.Property(o => o.Id).HasConversion(v => int.Parse(v), v => v.ToString());
}
