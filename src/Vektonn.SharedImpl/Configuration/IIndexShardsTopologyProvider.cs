using System.Collections.Generic;
using System.Net;
using Vektonn.SharedImpl.Contracts;

namespace Vektonn.SharedImpl.Configuration
{
    public interface IIndexShardsTopologyProvider
    {
        Dictionary<string, DnsEndPoint> GetEndpointsByShardIdForIndex(IndexId indexId);
    }
}
