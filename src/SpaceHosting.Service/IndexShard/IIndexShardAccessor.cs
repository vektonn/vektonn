using SpaceHosting.Contracts;
using SpaceHosting.Contracts.ApiModels;
using SpaceHosting.IndexShard;

namespace SpaceHosting.Service.IndexShard
{
    public interface IIndexShardAccessor
    {
        IndexMeta IndexMeta { get; }

        ISearchQueryExecutor SearchQueryExecutor { get; }

        long DataPointsCount { get; }

        VectorDto ZeroVector { get; }
    }
}
