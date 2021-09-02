using SpaceHosting.Index;
using SpaceHosting.IndexShard.Models;

namespace SpaceHosting.IndexShard.Service
{
    public interface ISourceMessageDecoder<TVector, in TSourceKey, in TSourceMessage>
        where TVector : IVector
    {
        DataPointOrTombstone<TVector> Decode(TSourceKey sourceKey, TSourceMessage? sourceMessage);
    }
}
