using NetCoreForce.Client;

namespace Ryan.Integration.Salesforce.Client;

public sealed record SalesforceAuthenticationClient(SalesforceConfiguration Configuration)
{
    public async Task<SalesforceCredentials> AuthenticateAsync()
    {
        var auth = new AuthenticationClient();

        await auth.TokenRefreshAsync(
            Configuration.RefreshToken,
            Configuration.ClientId,
            Configuration.ClientSecret,
            Configuration.LoginUrl);

        return new SalesforceCredentials(
            auth.AccessInfo.AccessToken,
            auth.AccessInfo.InstanceUrl,
            auth.ApiVersion);
    }
}

public sealed record SalesforceCredentials(
    string AccessToken,
    string InstanceUrl,
    string ApiVersion
);