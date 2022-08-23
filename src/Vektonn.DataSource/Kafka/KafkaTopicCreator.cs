using System;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Vektonn.SharedImpl.Json;
using Vostok.Logging.Abstractions;

namespace Vektonn.DataSource.Kafka
{
    // todo (andrew, 25.09.2021): test
    internal class KafkaTopicCreator : IDisposable
    {
        private readonly ILog log;
        private readonly KafkaTopicCreationConfig kafkaTopicCreationConfig;
        private readonly IAdminClient adminClient;

        public KafkaTopicCreator(ILog log, KafkaProducerConfig kafkaProducerConfig)
        {
            this.log = log;
            kafkaTopicCreationConfig = kafkaProducerConfig.TopicCreationConfig;

            var adminClientConfig = kafkaTopicCreationConfig.GetAdminClientConfig(kafkaProducerConfig.BootstrapServers);

            adminClient = BuildAdmin(adminClientConfig);
        }

        private IAdminClient BuildAdmin(AdminClientConfig adminClientConfig)
        {
            return new AdminClientBuilder(adminClientConfig)
                .SetErrorHandler(
                    (_, error) =>
                    {
                        if (error.IsFatal)
                        {
                            LogExtensions.Error(log, $"ConfluentAdminClient error: {error.ToPrettyJson()}");
                        }
                        else
                        {
                            LogExtensions.Warn(log, $"ConfluentAdminClient warn: {error.ToPrettyJson()}");
                        }
                    })
                .SetLogHandler(
                    (_, logMessage) => LogExtensions.Info(log, $"ConfluentAdminClient log message: {logMessage.ToPrettyJson()}"))
                .Build();
        }

        public void Dispose()
        {
            adminClient.Dispose();
        }

        public async Task CreateTopicAsync(string topicName, TimeSpan timeout)
        {
            var topicSpecification = kafkaTopicCreationConfig.BuildTopicSpecification(topicName);

            var createTopicsOptions = new CreateTopicsOptions
            {
                RequestTimeout = timeout,
                OperationTimeout = timeout
            };

            try
            {
                await adminClient.CreateTopicsAsync(new[] {topicSpecification}, createTopicsOptions);
                log.Info($"Successfully created topic '{topicName}' with topicSpecification: {topicSpecification.ToPrettyJson()}");
            }
            catch (CreateTopicsException e)
            {
                if (e.Results.Single().Error.Code != ErrorCode.TopicAlreadyExists)
                    throw;

                log.Warn(e, $"Topic '{topicName}' already exists");
            }
        }
    }
}
