using Vektonn.ApiContracts;
using Vektonn.IndexShard;
using Vektonn.SharedImpl.Contracts;

namespace Vektonn.Service.IndexShard
{
    public interface IIndexShardAccessor
    {
        IndexMeta IndexMeta { get; }

        ISearchQueryExecutor SearchQueryExecutor { get; }

        long DataPointsCount { get; }

        VectorDto ZeroVector { get; }
    }
}
