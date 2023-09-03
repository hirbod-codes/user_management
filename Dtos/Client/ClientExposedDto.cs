using MongoDB.Bson;
using user_management.Validation.Attributes;

namespace user_management.Dtos.Client;

public class ClientExposedDto
{
    [ObjectId]
    public ObjectId ClientId { get; set; }
    public string Secret { get; set; } = null!;
}