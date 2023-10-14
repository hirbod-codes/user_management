using user_management.Validation.Attributes;

namespace user_management.Dtos.Client;

public class ClientPublicInfoRetrieveDto
{
    [ObjectId]
    public string? Id { get; set; }
    public string? RedirectUrl { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
