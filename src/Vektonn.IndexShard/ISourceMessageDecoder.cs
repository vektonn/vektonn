using Vektonn.Contracts;
using Vektonn.Index;

namespace Vektonn.IndexShard
{
    public interface ISourceMessageDecoder<TVector, in TSourceKey, in TSourceMessage>
        where TVector : IVector
    {
        DataPointOrTombstone<TVector> Decode(TSourceKey sourceKey, TSourceMessage? sourceMessage);
    }
}
