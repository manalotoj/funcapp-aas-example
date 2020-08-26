using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Aas.FuncApp.Services;
using System.Net.Http;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;

namespace Aas.FuncApp.Functions
{
  public class WebAppHttpTrigger
  {
    private readonly IConfiguration config;
    private readonly AzureAdService adService;
    private readonly AzureWebSiteService webAppService;
    private readonly ILogger<WebAppHttpTrigger> log;

    public WebAppHttpTrigger(IConfiguration config, AzureAdService adService, AzureWebSiteService webAppService, ILogger<WebAppHttpTrigger> log)
    {
      this.config = config;
      this.adService = adService;
      this.webAppService = webAppService;
      this.log = log;
    }

    [FunctionName(nameof(GetPossibleOutboundIpAddresses))]
    public async Task<IActionResult> GetPossibleOutboundIpAddresses(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "subscriptions/{subscriptionId}/groups/{group}/webApps/{app}/possibleOutboundIpAddresses")] HttpRequestMessage req,
        string subscriptionId, string group, string app)
    {
      log.LogTrace($"subscriptionId: {subscriptionId}, resource group: {group}, app: {app}");
      var accessToken = await adService.GetAccessTokenAsync();
      log.LogTrace($"accessToken: {accessToken}");

      var managedIdentityId = config.GetValue<string>("ManagedIdentityId");
      var connectionString = "RunAs=App;AppId=" + managedIdentityId + $";TenantId={config.GetValue<string>("tenantId")}";
      var tokenProvider = new AzureServiceTokenProvider(connectionString);
      accessToken = await tokenProvider.GetAccessTokenAsync(config.GetValue<string>("ClientScope"));


      var credentials = adService.GetCredentials();
      var ipAddressList = await webAppService.GetPossibleOutboundIpAddressesAsync(credentials, subscriptionId, group, app);
      log.LogTrace($"possible outbound Ip Addresses: {ipAddressList}");

      return new OkObjectResult(ipAddressList);
    }
  }
}
