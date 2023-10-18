using System.Text.Json.Serialization;
using user_management.Models;
using user_management.Validation.Attributes;
using Bogus;
using Swashbuckle.AspNetCore.Filters;
using user_management.Data;

namespace user_management.Dtos.User;

public class UserClientRetrieveDto : IExamplesProvider<UserClientRetrieveDto>
{
    [ObjectId]
    [JsonPropertyName(UserClient.CLIENT_ID)]
    public string? ClientId { get; set; }
    [JsonPropertyName(UserClient.REFRESH_TOKEN)]
    public RefreshToken? RefreshToken { get; set; }
    [JsonPropertyName(UserClient.TOKEN)]
    public Models.Token? Token { get; set; }

    public UserClientRetrieveDto GetExamples() => new()
    {
        ClientId = new Faker().Random.String2(24, "0123456789"),
        RefreshToken = new()
        {
            Value = new Faker().Random.String2(128),
            TokenPrivileges = new()
            {
                Privileges = new Faker().PickRandom(StaticData.Privileges, 3).ToArray(),
                ReadsFields = new Faker().PickRandom(Models.User.GetReadableFields(), 3).ToArray(),
                UpdatesFields = new Faker().PickRandom(Models.User.GetUpdatableFields(), 3).ToArray(),
                DeletesUser = new Faker().Random.Bool()
            },
            ExpirationDate = new Faker().Date.Between(DateTime.UtcNow.AddDays(4), DateTime.UtcNow.AddDays(7)),
        },
        Token = new()
        {
            Value = new Faker().Random.String2(128),
            IsRevoked = new Faker().Random.Bool(),
            ExpirationDate = new Faker().Date.Between(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(3)),
        },
    };
}
