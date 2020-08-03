using System.Threading.Tasks;
using Aas.FuncApp.Models;

namespace Aas.FuncApp.Entities
{
  public interface IAnalysisServerManager
  {
    Task UpdateFirewallSettings(UpdateRequestMessage requestMessage);
  }
}
