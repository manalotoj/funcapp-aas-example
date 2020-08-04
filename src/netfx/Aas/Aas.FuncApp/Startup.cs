using System;
using System.IO;
using System.Net.Http;
using Aas.FuncApp.Entities;
using Aas.FuncApp.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

[assembly: FunctionsStartup(typeof(Aas.FuncApp.Startup))]
namespace Aas.FuncApp
{
  public class Startup : FunctionsStartup
  {
    public override void Configure(IFunctionsHostBuilder builder)
    {
      Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Debug()
        .CreateLogger();

      try
      {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddEnvironmentVariables()
            .Build();

        builder.Services.AddSingleton<IConfiguration>(config);
        builder.Services.AddLogging(
          lb => lb.ClearProviders()
            .AddSerilog(Log.Logger));


        var httpClient = new HttpClient();
        builder.Services.AddSingleton(httpClient);

        builder.Services.AddScoped(typeof(AzureAnalysisService));
        builder.Services.AddScoped(typeof(AzureAdService));
        builder.Services.AddScoped(typeof(AzureWebSiteService));
        builder.Services.AddScoped(typeof(AnalysisServiceManager));
      }
      catch (Exception ex)
      {
        Log.Fatal(ex, "Host terminated unexpectedly");
        throw;
      }
      finally
      {
        Log.CloseAndFlush();
      }
    }


    public bool IsDevelopmentEnvironment()
    {
      return "Development".Equals(Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT"), StringComparison.OrdinalIgnoreCase);
    }
  }
}
