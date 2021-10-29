using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Vektonn.Hosting;
using Vostok.Logging.Abstractions;

namespace Vektonn.IndexShardService.Services
{
    public class IndexShardHostedService : IHostedService
    {
        private readonly ILog log;
        private readonly IShutdownTokenProvider shutdownTokenProvider;
        private readonly IIndexShardOwner indexShardOwner;

        public IndexShardHostedService(ILog log, IShutdownTokenProvider shutdownTokenProvider, IIndexShardOwner indexShardOwner)
        {
            this.log = log;
            this.shutdownTokenProvider = shutdownTokenProvider;
            this.indexShardOwner = indexShardOwner;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            log.Info("IndexShardService initialization started");
            await indexShardOwner.DataSourceConsumer.RunAsync(shutdownTokenProvider.HostShutdownToken);
            log.Info("IndexShardService initialization completed");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            log.Info("IndexShardService disposal started");
            indexShardOwner.Dispose();
            log.Info("IndexShardService disposal completed");

            return Task.CompletedTask;
        }
    }
}
