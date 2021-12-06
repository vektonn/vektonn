using System.Collections.Generic;
using System.Net;

namespace Vektonn.SharedImpl.Contracts
{
    public record IndexMetaWithShardEndpoints(IndexMeta IndexMeta, Dictionary<string, DnsEndPoint> EndpointsByShardId);
}
