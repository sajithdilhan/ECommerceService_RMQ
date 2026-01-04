namespace Shared.Authentication;

public class AuthenticationOptions
{
    public const string SectionName = "Authentication";
    public string ApiKey { get; set; } = string.Empty;
}
