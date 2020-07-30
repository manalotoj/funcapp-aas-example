using Aas.FuncApp.Models;

namespace Aas.FuncApp.Entities
{
  public class UpdateRequestMessage
  {
    public string AccessToken { get; set; }
    public UpdateFirewallRulesRequestMessage UpdateFirewallRulesRequestMessage { get; set; }
    public IpV4FirewallSettings FirewallSettings { get; set; }
    public string OutboundIpAddresses { get; set; }
  }
}
