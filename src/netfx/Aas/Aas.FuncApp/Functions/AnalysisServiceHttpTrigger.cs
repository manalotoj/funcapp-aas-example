using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Aas.FuncApp.Models;
using Aas.FuncApp.Services;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Net.Http;
using Aas.FuncApp.Entities;
using System.Net;

namespace Aas.FuncApp.Functions
{
  public class AnalysisServiceHttpTrigger
  {
    private readonly AzureAdService aadService;
    private readonly AzureAnalysisService analysisService;
    private readonly AzureWebSiteService webAppService;
    private readonly ILogger<AnalysisServiceHttpTrigger> log;

    public AnalysisServiceHttpTrigger(AzureAdService aadService,
      AzureAnalysisService analysisService,
      AzureWebSiteService webAppService,
      ILogger<AnalysisServiceHttpTrigger> log)
    {
      this.aadService = aadService;
      this.analysisService = analysisService;
      this.webAppService = webAppService;
      this.log = log;
    }

    /// <summary>
    /// Updates firewall rules with the current possible outbound IP addresses of the provided Azure web application.
    /// </summary>
    /// <param name="req"></param>
    /// <param name="durableClient"></param>
    /// <returns></returns>
    [FunctionName(nameof(UpdateFirewallSettings))]
    public async Task<IActionResult> UpdateFirewallSettings(
      [HttpTrigger(AuthorizationLevel.Function, "patch", "post", Route = "")] HttpRequestMessage req,
       [DurableClient] IDurableClient durableClient)
    {
      var input = await req.Content.ReadAsAsync<UpdateFirewallRulesRequestMessage>();

      var ipAddresses = await GetPossibleOutboundIpAddresses(input);
      var accessToken = await aadService.GetAccessTokenAsync();
      var analyisServerSettings =
        await analysisService.GetServerAsync(accessToken,
          input.analysisServerSubscriptionId,
          input.analysisServerResourceGroup,
          input.analysisServerName);

      UpdateRequestMessage updateRequest = new UpdateRequestMessage {
        AccessToken = accessToken,
        FirewallSettings = analyisServerSettings.properties.ipV4FirewallSettings,
        OutboundIpAddresses = ipAddresses,
        UpdateFirewallRulesRequestMessage = input
      };

      var entityId = new EntityId(nameof(AnalysisServiceManager), $"{input.analysisServerSubscriptionId}~{input.analysisServerResourceGroup}~{input.analysisServerName}");
      await durableClient.SignalEntityAsync<IAnalysisServerManager>(entityId, proxy =>
        proxy.UpdateFirewallSettings(updateRequest));
      return new AcceptedResult();
    }

    private async Task<string> GetPossibleOutboundIpAddresses(UpdateFirewallRulesRequestMessage input)
    {
      var accessToken = await aadService.GetAccessTokenAsync();
      log.LogTrace($"accessToken: {accessToken}");

      var credentials = aadService.GetCredentials();
      var ipAddressList = await webAppService.GetPossibleOutboundIpAddressesAsync(credentials, input.webAppSubscriptionId, input.webAppResourceGroup, input.webAppName);
      log.LogTrace($"possible outbound Ip Addresses: {ipAddressList}");
      return ipAddressList;
    }

    /// <summary>
    /// Replaces existing firewall rules with the firewall rules provided in the body of the request.
    /// </summary>
    /// <param name="req"></param>
    /// <param name="subscriptionId"></param>
    /// <param name="group"></param>
    /// <param name="server"></param>
    /// <returns></returns>
    [FunctionName(nameof(SetFirewallSettings))]
    public async Task<IActionResult> SetFirewallSettings(
        [HttpTrigger(AuthorizationLevel.Function, "patch", "post", Route = "subscriptions/{subscriptionId}/groups/{group}/analysisservers/{server}/firewallsettings")] HttpRequestMessage req, string subscriptionId, string group, string server)
    {
      IpV4FirewallSettings firewallSettings = await req.Content.ReadAsAsync<IpV4FirewallSettings>();
      if (firewallSettings == null) return new BadRequestObjectResult(new ArgumentNullException("message body invalid or not found"));

      var accessToken = await aadService.GetAccessTokenAsync();
      var result = await analysisService.SetFirewallRulesAsync(accessToken, subscriptionId, group, server, firewallSettings);

      if (result.IsSuccessStatusCode)
      {
        return new OkResult();
      }
      else
      {
        return new BadRequestObjectResult(result.ReasonPhrase);
      }
    }
  }
}
