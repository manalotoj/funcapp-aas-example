using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Aas.FuncApp.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Aas.FuncApp.Services
{
  public class AzureAnalysisService
  {
    private readonly HttpClient httpClient;
    private readonly ILogger<AzureAnalysisService> log;
    private const string webApiUrlFormat = "https://management.azure.com/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.AnalysisServices/servers/{2}?api-version=2017-08-01";

    public AzureAnalysisService(HttpClient httpClient, ILogger<AzureAnalysisService> log)
    {
      this.httpClient = httpClient;
      this.log = log;
    }

    public async Task<AzureAnalysisServer> GetServerAsync(string accessToken, string subscriptionId, string resourceGroup, string serverName)
    {
      var defaultRequestHeaders = httpClient.DefaultRequestHeaders;
      if (defaultRequestHeaders.Accept == null || !defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
      {
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      }
      defaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", accessToken);

      HttpResponseMessage response = await httpClient.GetAsync(string.Format(webApiUrlFormat, subscriptionId, resourceGroup, serverName));
      var server = JsonConvert.DeserializeObject<AzureAnalysisServer>(await response.Content.ReadAsStringAsync());
      return server;
    }

    public async Task<HttpResponseMessage> SetFirewallRulesAsync(string accessToken, string subscriptionId, string resourceGroup, string serverName, IpV4FirewallSettings firewallSettings)
    {
      log.LogTrace(JsonConvert.SerializeObject(firewallSettings));
      var defaultRequestHeaders = httpClient.DefaultRequestHeaders;
      if (defaultRequestHeaders.Accept == null || !defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
      {
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      }
      defaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", accessToken);

      var server = new AzureAnalysisServer { properties = new Properties { ipV4FirewallSettings = firewallSettings } };
      var json = JsonConvert.SerializeObject(server);
      var content = new StringContent(json, Encoding.UTF8, "application/json");
      return await httpClient.PatchAsync(string.Format(webApiUrlFormat, subscriptionId, resourceGroup, serverName), content);
    }
  }
}
