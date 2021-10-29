using System.Threading.Tasks;
using Vektonn.Hosting;

namespace Vektonn.ApiService
{
    public static class ApiServiceEntryPoint
    {
        public static async Task Main()
        {
            var hostSettings = VektonnApplicationHostSettings.FromEnvironmentVariables();
            var applicationHost = new VektonnApplicationHost<ApiServiceApplication>(hostSettings);
            await applicationHost.RunToCompletionAsync();
        }
    }
}
