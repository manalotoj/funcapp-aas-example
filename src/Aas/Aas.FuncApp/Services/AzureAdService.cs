using System;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace Aas.FuncApp.Services
{
  public class AzureAdService
  {
    private readonly string clientId;
    private readonly string clientSecret;
    private readonly string authority;
    private readonly string scope;
    private readonly string tenantId;
    private readonly string loginUrl;
    private readonly ILogger<AzureAdService> log;

    public AzureAdService(IConfiguration config, ILogger<AzureAdService> log)
    {
      clientId = config.GetValue<string>("ClientId");
      clientSecret = config.GetValue<string>("ClientSecret");
      scope = config.GetValue<string>("ClientScope");
      tenantId = config.GetValue<string>("TenantId");
      loginUrl = config.GetValue<string>("LoginUrl");
      authority = $"{loginUrl}/{tenantId}";
      this.log = log;

      //scope = "https://management.azure.com/.default";
    }

    public async Task<string> GetAccessTokenAsync()
    {
      IConfidentialClientApplication app;
      app = ConfidentialClientApplicationBuilder.Create(clientId)
                    .WithClientSecret(clientSecret)
                    .WithAuthority(new Uri(authority))
                    .Build();

      AuthenticationResult result = await app.AcquireTokenForClient(new string[] { scope })
        .ExecuteAsync();

      log.LogTrace($"Access token retrieved from Azure AD: {result.AccessToken}");

      return result.AccessToken;
    }

    public AzureCredentials GetCredentials()
    {
      var credentials = SdkContext.AzureCredentialsFactory
        .FromServicePrincipal(clientId,
            clientSecret,
            tenantId,
            AzureEnvironment.AzureGlobalCloud);
      return credentials;
    }
  }
}
