using System;
using System.IO;
using System.Net.Http;
using Aas.FuncApp.Entities;
using Aas.FuncApp.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Aas.FuncApp.Startup))]
namespace Aas.FuncApp
{
  public class Startup : FunctionsStartup
  {
    public IServiceCollection Services => throw new NotImplementedException();

    public override void Configure(IFunctionsHostBuilder builder)
    {
      var config = new ConfigurationBuilder()
          .SetBasePath(Directory.GetCurrentDirectory())
          .AddEnvironmentVariables()
          .Build();

      builder.Services.AddSingleton<IConfiguration>(config);
      builder.Services.AddLogging();

      var httpClient = new HttpClient();
      builder.Services.AddSingleton(httpClient);

      builder.Services.AddScoped(typeof(AzureAnalysisService));
      builder.Services.AddScoped(typeof(AzureAdService));
      builder.Services.AddScoped(typeof(AzureWebSiteService));
      builder.Services.AddScoped(typeof(AnalysisServiceManager));
    }

    public bool IsDevelopmentEnvironment()
    {
      return "Development".Equals(Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT"), StringComparison.OrdinalIgnoreCase);
    }
  }
}
