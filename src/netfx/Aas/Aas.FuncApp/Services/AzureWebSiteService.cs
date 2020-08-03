using System.Threading.Tasks;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Aas.FuncApp.Services
{
  public class AzureWebSiteService
  {
    private readonly ILogger<AzureWebSiteService> log;

    public AzureWebSiteService(ILogger<AzureWebSiteService> log)
    {
      this.log = log;
    }

    public async Task<string> GetPossibleOutboundIpAddressesAsync(AzureCredentials credentials, string subscriptionId, string rgName, string resourceName)
    {
      log.LogTrace($"Retrieving possible outbound IP addresses for subscription '{subscriptionId}, resource group '{rgName}' and web site '{resourceName}'");
;
      using (var client = GetWebSiteManagementClient(credentials, subscriptionId))
      {
        var result = await client.WebApps.GetAsync(rgName, resourceName);
        log.LogTrace($"possible outbound IP addresses: {result.PossibleOutboundIpAddresses}");
        return result.PossibleOutboundIpAddresses;
      }
    }

    private WebSiteManagementClient GetWebSiteManagementClient(AzureCredentials credentials, string subscriptionId)
    {
      var restClient = RestClient.Configure()
        .WithEnvironment(AzureEnvironment.AzureGlobalCloud)
        .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
        .WithCredentials(credentials)
        .Build();

      var webSiteMgmtClient = new WebSiteManagementClient(restClient);
      webSiteMgmtClient.SubscriptionId = subscriptionId;
      return webSiteMgmtClient;
    }
  }
}
