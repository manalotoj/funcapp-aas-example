using System.Collections.Generic;

namespace Aas.FuncApp.Models
{
  public class FirewallRule
  {
    public string firewallRuleName { get; set; }
    public string rangeStart { get; set; }
    public string rangeEnd { get; set; }

  }

  public class IpV4FirewallSettings
  {
    public List<FirewallRule> firewallRules { get; set; }
    public bool enablePowerBIService { get; set; }

  }

  public class Properties
  {
    public IpV4FirewallSettings ipV4FirewallSettings { get; set; }

  }

  public class AzureAnalysisServer
  {
    public Properties properties { get; set; }
  }
}
