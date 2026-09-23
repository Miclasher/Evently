using System.Text.Json.Serialization;

namespace Evently.Modules.Users.Infrastructure.Identity;

public sealed class AuthToken
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; }
}
