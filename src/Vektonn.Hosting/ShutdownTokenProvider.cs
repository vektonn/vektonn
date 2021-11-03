using System.Threading;
using Vostok.Hosting.Abstractions;

namespace Vektonn.Hosting
{
    public class ShutdownTokenProvider : IShutdownTokenProvider
    {
        public ShutdownTokenProvider(IVostokHostingEnvironment hostingEnvironment)
        {
            HostShutdownToken = hostingEnvironment.ShutdownToken;
        }

        public CancellationToken HostShutdownToken { get; }
    }
}
