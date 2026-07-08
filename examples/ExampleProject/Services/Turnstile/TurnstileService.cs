using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ExampleProject.Services.Turnstile;

public class TurnstileService(HttpClient httpClient, IOptions<TurnstileOptions> settings)
{
    private const string VerifyUrl = "https://challenges.cloudflare.com/turnstile/v0/siteverify";

    public async Task<bool> VerifyAsync(string? token, string? remoteIp = null)
    {
        if (!settings.Value.IsEnabled)
            return true;

        if (string.IsNullOrWhiteSpace(token))
            return false;

        var fields = new Dictionary<string, string>
        {
            ["secret"] = settings.Value.SecretKey,
            ["response"] = token
        };

        if (!string.IsNullOrWhiteSpace(remoteIp))
            fields["remoteip"] = remoteIp;

        var response = await httpClient.PostAsync(VerifyUrl, new FormUrlEncodedContent(fields));
        var body = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TurnstileResponse>(body);
        return result?.Success == true;
    }

    private class TurnstileResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }
}
