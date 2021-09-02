using SpaceHosting.Contracts;
using SpaceHosting.Index;

namespace SpaceHosting.IndexShard
{
    public interface ISourceMessageDecoder<TVector, in TSourceKey, in TSourceMessage>
        where TVector : IVector
    {
        DataPointOrTombstone<TVector> Decode(TSourceKey sourceKey, TSourceMessage? sourceMessage);
    }
}
