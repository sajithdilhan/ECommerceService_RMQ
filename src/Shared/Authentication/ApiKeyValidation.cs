using Microsoft.Extensions.Options;

namespace Shared.Authentication;

public class ApiKeyValidation(IOptions<AuthenticationOptions> authOptions) : IApiKeyValidation
{
    public bool IsValidApiKey(string? apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return false;
        }
        return apiKey == authOptions.Value.ApiKey;
    }
}
