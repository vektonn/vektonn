using System;
using System.Linq;
using Vostok.Clusterclient.Core.Model;

namespace Vektonn.ApiClient.HttpClusterClient
{
    public class VektonnClusterClientException : Exception
    {
        public VektonnClusterClientException(ClusterResult clusterResult)
            : base($"Request failed: {Format(clusterResult)}")
        {
        }

        private static string Format(ClusterResult clusterResult)
        {
            return $"Request: {clusterResult.Request.ToString(includeQuery: true, includeHeaders: true)}\n" +
                   $"Status: {clusterResult.Status}\n" +
                   $"Selected response: {clusterResult.Response}\n" +
                   $"Selected replica: {clusterResult.Replica}\n" +
                   $"Replica results: [{string.Join(", \n", clusterResult.ReplicaResults.Select(Format))}]";
        }

        private static string Format(ReplicaResult replicaResult)
        {
            var responseContent = replicaResult.Response.HasContent ? replicaResult.Response.Content : null;
            return $"Replica: {replicaResult.Replica}\n" +
                   $"Response: {replicaResult.Response}\n" +
                   $"Content: {responseContent}\n" +
                   $"Verdict: {replicaResult.Verdict}\n" +
                   $"Time: {replicaResult.Time}";
        }
    }
}
