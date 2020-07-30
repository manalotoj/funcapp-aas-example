namespace Aas.FuncApp.Models
{
  public class UpdateFirewallRulesRequestMessage
  {
    public string analysisServerName { get; set; }
    public string analysisServerResourceGroup { get; set; }
    public string analysisServerSubscriptionId { get; set; }
    public string webAppName { get; set; }
    public string webAppResourceGroup { get; set; }
    public string webAppSubscriptionId { get; set; }
  }
}
