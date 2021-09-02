using System.Collections.Generic;
using System.Linq;
using SpaceHosting.Contracts;
using SpaceHosting.Index;

namespace SpaceHosting.IndexShard.Shard
{
    public static class IndexDataPointSerializer
    {
        public static IndexDataPointOrTombstone<byte[], byte[], TVector>[] ToIndexDataPointOrTombstones<TVector>(
            this IEnumerable<DataPointOrTombstone<TVector>> batch,
            AttributesAccessor attributesAccessor
        )
            where TVector : IVector
        {
            return batch.Select(
                    x =>
                    {
                        var indexId = attributesAccessor.GetIndexId(x.GetAttributes());
                        var idBytes = AttributeValueSerializer.Serialize(indexId);

                        if (x.Tombstone != null)
                            return new IndexDataPointOrTombstone<byte[], byte[], TVector>(new IndexTombstone<byte[]>(idBytes));

                        var payload = attributesAccessor.TryGetPayload(x.DataPoint!.Attributes);
                        return new IndexDataPointOrTombstone<byte[], byte[], TVector>(
                            new IndexDataPoint<byte[], byte[], TVector>(
                                Id: idBytes,
                                Data: payload == null ? null : AttributeValueSerializer.Serialize(payload),
                                Vector: x.DataPoint!.Vector));
                    })
                .ToArray();
        }

        public static IReadOnlyList<SearchResultItem<TVector>> ToSearchResults<TVector>(
            this IEnumerable<IndexSearchResultItem<byte[], byte[], TVector>> queryResults,
            AttributesAccessor attributesAccessor,
            byte[]? splitKeyBytes
        )
            where TVector : IVector
        {
            return queryResults.Select(
                    queryResult =>
                    {
                        var nearestDataPoints = queryResult.NearestDataPoints.Select(
                                fdp =>
                                {
                                    var indexId = AttributeValueSerializer.Deserialize(fdp.Id);
                                    var splitKey = splitKeyBytes == null ? null : AttributeValueSerializer.Deserialize(splitKeyBytes);
                                    var payload = fdp.Data == null ? null : AttributeValueSerializer.Deserialize(fdp.Data);
                                    var attributes = attributesAccessor.GetAttributes(indexId, splitKey, payload);
                                    return new FoundDataPoint<TVector>(fdp.Vector, attributes, fdp.Distance);
                                })
                            .ToArray();
                        return new SearchResultItem<TVector>(queryResult.QueryVector, nearestDataPoints);
                    })
                .ToArray();
        }
    }
}
