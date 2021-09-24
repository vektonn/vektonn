using Vektonn.Contracts;
using Vektonn.Contracts.ApiModels;
using Vektonn.IndexShard;

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
