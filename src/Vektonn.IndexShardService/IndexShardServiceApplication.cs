using System;
using Microsoft.Extensions.DependencyInjection;
using Vektonn.Hosting;
using Vektonn.Index;
using Vektonn.IndexShardService.Services;
using Vektonn.SharedImpl.Json;
using Vostok.Logging.Abstractions;

namespace Vektonn.IndexShardService
{
    public class IndexShardServiceApplication : VektonnApplicationBase
    {
        public sealed override string ApplicationName { get; } = "IndexShardService";

        protected sealed override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IndexShardConfigurationProvider>();

            services.AddSingleton<IIndexShardOwner>(
                serviceProvider =>
                {
                    var log = serviceProvider.GetRequiredService<ILog>();
                    var indexShardConfigurationProvider = serviceProvider.GetRequiredService<IndexShardConfigurationProvider>();

                    var indexShardConfiguration = indexShardConfigurationProvider.GetConfiguration();
                    log.Info($"Using indexShardConfiguration: {indexShardConfiguration.ToPrettyJson()}");

                    return AlgorithmTraits.VectorsAreSparse(indexShardConfiguration.IndexMeta.IndexAlgorithm.Type)
                        ? new IndexShardOwner<SparseVector>(
                            log,
                            indexShardConfiguration,
                            zeroVector: new SparseVector(indexShardConfiguration.IndexMeta.VectorDimension, Array.Empty<double>(), Array.Empty<int>()))
                        : new IndexShardOwner<DenseVector>(
                            log,
                            indexShardConfiguration,
                            zeroVector: new DenseVector(new double[indexShardConfiguration.IndexMeta.VectorDimension]));
                });
            services.AddSingleton<IIndexShardAccessor>(s => s.GetRequiredService<IIndexShardOwner>());

            services.AddHostedService<IndexShardHostedService>();
        }
    }
}
