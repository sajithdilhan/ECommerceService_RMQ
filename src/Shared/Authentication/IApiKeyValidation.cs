namespace Shared.Authentication;

public interface IApiKeyValidation
{
    bool IsValidApiKey(string? apiKey);
}
