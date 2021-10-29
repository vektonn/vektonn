using System.Threading.Tasks;
using Vektonn.Hosting;

namespace Vektonn.IndexShardService
{
    public static class IndexShardServiceEntryPoint
    {
        public static async Task Main()
        {
            var hostSettings = VektonnApplicationHostSettings.FromEnvironmentVariables();
            var applicationHost = new VektonnApplicationHost<IndexShardServiceApplication>(hostSettings);
            await applicationHost.RunToCompletionAsync();
        }
    }
}
