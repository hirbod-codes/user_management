using user_management.Validation.Attributes;

namespace user_management.Dtos.Client;

public class ClientDeleteDto
{
    [ObjectId]
    public string Id { get; set; } = null!;
    public string Secret { get; set; } = null!;
}