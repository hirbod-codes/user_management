using System.Text.Json.Serialization;
using user_management.Models;
using user_management.Validation.Attributes;

namespace user_management.Dtos.User;

public class UserClientRetrieveDto
{
    [ObjectId]
    [JsonPropertyName(UserClient.CLIENT_ID)]
    public string? ClientId { get; set; }
    [JsonPropertyName(UserClient.REFRESH_TOKEN)]
    public RefreshToken? RefreshToken { get; set; }
    [JsonPropertyName(UserClient.TOKEN)]
    public Models.Token? Token { get; set; }
}
