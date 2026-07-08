namespace ExampleProject.Services.Turnstile;

public class TurnstileOptions
{
    public string SiteKey { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public bool Enabled { get; set; }

    public bool IsEnabled => Enabled && !string.IsNullOrWhiteSpace(SiteKey) && !string.IsNullOrWhiteSpace(SecretKey);
}
