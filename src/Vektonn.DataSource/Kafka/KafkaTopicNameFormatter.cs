using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Vektonn.SharedImpl.Contracts;

namespace Vektonn.DataSource.Kafka
{
    // todo (andrew, 25.09.2021): test
    internal static class KafkaTopicNameFormatter
    {
        private const string TopicNamePrefix = "Vektonn";
        private const string AnySegmentPattern = "[^.]+";

        public static string FormatTopicName(DataSourceId dataSourceId, Dictionary<string, ulong> shardingCoordinatesByAttributeKey)
        {
            return FormatTopicNameOrPattern(dataSourceId, shardingCoordinatesByAttributeKey.ToDictionary(t => t.Key, t => (ulong?)t.Value));
        }

        public static string FormatTopicNameOrPattern(DataSourceId dataSourceId, Dictionary<string, ulong?> shardingCoordinatesByAttributeKey)
        {
            var sb = new StringBuilder($"{TopicNamePrefix}.{Normalize(dataSourceId.Name)}-{Normalize(dataSourceId.Version)}");

            var useRegex = false;
            foreach (var (attributeKey, shardingCoordinate) in shardingCoordinatesByAttributeKey.OrderBy(t => t.Key, StringComparer.InvariantCulture))
            {
                string shardingCoordinateSegment;
                if (shardingCoordinate != null)
                    shardingCoordinateSegment = shardingCoordinate.Value.ToString(CultureInfo.InvariantCulture);
                else
                {
                    useRegex = true;
                    shardingCoordinateSegment = AnySegmentPattern;
                }

                sb.Append($".{Normalize(attributeKey)}-{shardingCoordinateSegment}");
            }

            return useRegex ? $"^{sb}" : sb.ToString();
        }

        private static string Normalize(string topicNameSegment)
        {
            return topicNameSegment.Replace(".", "_");
        }
    }
}
