using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Vektonn.ApiClient.IndexShard;
using Vektonn.ApiService.Services;
using Vektonn.DataSource;
using Vektonn.DataSource.Kafka;
using Vektonn.Hosting;
using Vektonn.Hosting.Configuration;
using Vektonn.SharedImpl.Json;
using Vostok.Logging.Abstractions;

namespace Vektonn.ApiService
{
    public class ApiServiceApplication : VektonnApplicationBase
    {
        public sealed override string ApplicationName { get; } = "ApiService";

        protected sealed override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IndexShardApiClusterClient>();
            services.AddSingleton<IndexShardApiClientProvider>();

            services.AddSingleton<IDataSourceProducer>(
                serviceProvider =>
                {
                    var log = serviceProvider.GetRequiredService<ILog>();
                    var kafkaConfigurationProvider = serviceProvider.GetRequiredService<KafkaConfigurationProvider>();

                    var kafkaProducerConfig = new KafkaProducerConfig(
                        kafkaConfigurationProvider.GetKafkaBootstrapServers(),
                        new KafkaTopicCreationConfig(
                            kafkaConfigurationProvider.GetTopicReplicationFactor()));
                    log.Info($"Using kafkaProducerConfig: {kafkaProducerConfig.ToPrettyJson()}");

                    return new KafkaDataSourceProducer(log, kafkaProducerConfig);
                });

            services.AddSwaggerGen(
                swaggerGenOptions =>
                {
                    swaggerGenOptions.SupportNonNullableReferenceTypes();
                    swaggerGenOptions.UseOneOfForPolymorphism();
                    swaggerGenOptions.SwaggerDoc(
                        "v1",
                        new OpenApiInfo
                        {
                            Title = "Vektonn API V1",
                            Version = "v1",
                            License = new OpenApiLicense
                            {
                                Name = "Apache 2.0",
                                Url = new Uri("http://www.apache.org/licenses/LICENSE-2.0.html")
                            }
                        });
                });
        }

        protected sealed override void ConfigureApp(IApplicationBuilder app)
        {
            base.ConfigureApp(app);

            app.UseSwagger();
            app.UseSwaggerUI(
                swaggerUiOptions =>
                {
                    swaggerUiOptions.SwaggerEndpoint("/swagger/v1/swagger.json", "Vektonn API V1");
                    swaggerUiOptions.RoutePrefix = string.Empty;
                    swaggerUiOptions.DocumentTitle = "Vektonn API";
                    swaggerUiOptions.DefaultModelsExpandDepth(2);
                });
        }
    }
}
