using System;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace Aas.FuncApp.Services
{
  public class AzureAdService
  {
    //private readonly string clientId;
    //private readonly string clientSecret;
    private readonly string resource;
    private readonly string scope;
    private readonly IConfiguration config;

    private readonly string managedIdentityId;
    private readonly ILogger<AzureAdService> log;

    public AzureAdService(IConfiguration config, ILogger<AzureAdService> log)
    {
      scope = config.GetValue<string>("ClientScope");
      this.config = config;
      managedIdentityId = config.GetValue<string>("ManagedIdentityId");
      resource = config.GetValue<string>("Resource");
      this.log = log;

      //scope = "https://management.azure.com/.default";
    }

    public async Task<string> GetAccessTokenAsync()
    {
      /*
      IConfidentialClientApplication app;
      app = ConfidentialClientApplicationBuilder.Create(clientId)
                    .WithClientSecret(clientSecret)
                    .WithAuthority(new Uri(authority))
                    .Build();

      AuthenticationResult result = await app.AcquireTokenForClient(new string[] { scope })
        .ExecuteAsync();

      log.LogTrace($"Access token retrieved from Azure AD: {result.AccessToken}");

      return result.AccessToken;
      */
      var connectionString = "RunAs=App;AppId=" + managedIdentityId;
      var tokenProvider = new AzureServiceTokenProvider(connectionString);
      var accessToken = await tokenProvider.GetAccessTokenAsync(resource);
      log.LogInformation($"accesstoken from mi: {accessToken}");

      return accessToken;
    }

    public AzureCredentials GetCredentials()
    {
      var credentials = SdkContext.AzureCredentialsFactory
        .FromUserAssigedManagedServiceIdentity(
          managedIdentityId, 
          MSIResourceType.AppService, 
          AzureEnvironment.AzureGlobalCloud);

      return credentials;
    }
  }
}
