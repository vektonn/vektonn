using System;
using Vektonn.SharedImpl.Contracts;

namespace Vektonn.ApiService.Services
{
    public class IndexShardApiTopologyProvider
    {
        public Uri GetIndexShardBaseUri(IndexId indexId, string shardId)
        {
            return new Uri("http://vektonn-index-shard:8082");
        }
    }
}
