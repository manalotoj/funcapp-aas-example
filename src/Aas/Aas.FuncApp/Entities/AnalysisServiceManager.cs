using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aas.FuncApp.Models;
using Aas.FuncApp.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Aas.FuncApp.Entities
{
  [JsonObject(MemberSerialization.OptIn)]
  public class AnalysisServiceManager : IAnalysisServerManager
  {
    private readonly AzureAnalysisService analysisService;
    private readonly ILogger<AnalysisServiceManager> log;

    [JsonProperty("OutboundIpAddresses")]
    public string OutboundIpAddresses { get; set; }


    public AnalysisServiceManager(AzureAnalysisService analysisService, ILogger<AnalysisServiceManager> log)
    {
      this.analysisService = analysisService;
      this.log = log;
    }

    public async Task UpdateFirewallSettings(UpdateRequestMessage requestMessage)
    {
      var ipAddressesToRemove = GetIpAddressesToRemove(requestMessage.OutboundIpAddresses);
      var newFirewallRules = BuildNewFirewallRules(requestMessage.OutboundIpAddresses, ipAddressesToRemove, requestMessage.FirewallSettings.firewallRules);

      var result =
        await analysisService.SetFirewallRulesAsync(requestMessage.AccessToken,
          requestMessage.UpdateFirewallRulesRequestMessage.analysisServerSubscriptionId,
          requestMessage.UpdateFirewallRulesRequestMessage.analysisServerResourceGroup,
          requestMessage.UpdateFirewallRulesRequestMessage.analysisServerName,
          new IpV4FirewallSettings
          {
             enablePowerBIService = requestMessage.FirewallSettings.enablePowerBIService,
              firewallRules = newFirewallRules
          });

      if (result.IsSuccessStatusCode)
      {
        log.LogTrace($"Successfully updated firewall rules using {requestMessage.OutboundIpAddresses}");
        OutboundIpAddresses = requestMessage.OutboundIpAddresses;
      }
      else
      {
        var message = $"Failed to update firewall rules using {requestMessage.OutboundIpAddresses}; reason: {result.ReasonPhrase}";
        log.LogTrace(message);
        throw new Exception(message);
      }
    }

    private List<string> GetIpAddressesToRemove(string newIpAddressess)
    {
      var ipAddressesToRemove = new List<string>();
      if (string.IsNullOrEmpty(OutboundIpAddresses)) return ipAddressesToRemove;

      foreach (var address in OutboundIpAddresses.Split(','))
      {
        if (!newIpAddressess.Contains(address)) ipAddressesToRemove.Add(address);
      }
      return ipAddressesToRemove;
    }

    private List<FirewallRule> BuildNewFirewallRules(string newIpAddresses, List<string> ipAddressesToRemove, List<FirewallRule> existingFirewallRules)
    {
      var newFirewallRules = new List<FirewallRule>();
      newFirewallRules.AddRange(existingFirewallRules);

      foreach (var address in ipAddressesToRemove)
      {
        var item = existingFirewallRules.Where(rule => rule.rangeStart == address && rule.rangeEnd == address).FirstOrDefault();
        if (item != null) newFirewallRules.Remove(item);
      }

      foreach (var address in newIpAddresses.Split(','))
      {
        if (newFirewallRules.Where(rule => rule.rangeStart == address).FirstOrDefault() == null)
        {
          newFirewallRules.Add(
            new FirewallRule
            {
              firewallRuleName = address.Replace('.', '-'),
              rangeStart = address,
              rangeEnd = address
            });
        }
      }

      return newFirewallRules;
    }

    [FunctionName(nameof(AnalysisServiceManager))]
    public static Task Run([EntityTrigger] IDurableEntityContext ctx)
        => ctx.DispatchAsync<AnalysisServiceManager>();
  }
}