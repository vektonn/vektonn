using System.Threading;

namespace Vektonn.Hosting
{
    public interface IShutdownTokenProvider
    {
        CancellationToken HostShutdownToken { get; }
    }
}
