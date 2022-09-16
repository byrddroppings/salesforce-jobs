using Microsoft.Extensions.Configuration;

namespace Ryan.Integration.Salesforce.Client;

public sealed record SalesforceConfiguration(
    string BaseUrl,
    string ClientId,
    string ClientSecret,
    string RefreshToken
)
{
    public string LoginUrl => $"{BaseUrl}/services/oauth2/token";
}

public sealed record SalesforceConfigurationBuilder
{
    private string? _baseUrl;
    private string? _clientId;
    private string? _clientSecret;
    private string? _refreshToken;
    
    public SalesforceConfigurationBuilder WithConfiguration(
        IConfiguration config,
        string baseUrlKey = "Salesforce:BaseUrl",
        string clientIdKey = "Salesforce:Authentication:ClientId",
        string clientSecretKey = "Salesforce:Authentication:ClientSecret",
        string refreshTokenKey = "Salesforce:Authentication:RefreshToken"
    )
    {
        _baseUrl = config[baseUrlKey];
        _clientId = config[clientIdKey];
        _clientSecret = config[clientSecretKey];
        _refreshToken = config[refreshTokenKey];
        return this;
    }

    public SalesforceConfiguration Build()
    {
        VerifyNotNullOrWhitespace("BaseUrl",_baseUrl);
        VerifyNotNullOrWhitespace("ClientId",_clientId);
        VerifyNotNullOrWhitespace("ClientSecret",_clientSecret);
        VerifyNotNullOrWhitespace("RefreshToken",_refreshToken);

        return new SalesforceConfiguration(
            _baseUrl!,
            _clientId!,
            _clientSecret!,
            _refreshToken!
        );
    }

    private void VerifyNotNullOrWhitespace(string propertyName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new Exception($"No Salesforce {propertyName} provided");
        }
    }
}