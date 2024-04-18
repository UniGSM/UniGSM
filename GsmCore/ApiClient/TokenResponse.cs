using System.Text.Json.Serialization;

namespace GsmCore.ApiClient;

public class TokenResponse
{
    [JsonPropertyName("token")] public string Token { get; set; }
    [JsonPropertyName("expiration")] public DateTime Expiration { get; set; }
}