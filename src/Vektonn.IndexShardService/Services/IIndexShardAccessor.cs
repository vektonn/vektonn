using Vektonn.ApiContracts;
using Vektonn.IndexShard;
using Vektonn.SharedImpl.Contracts;

namespace Vektonn.IndexShardService.Services
{
    public interface IIndexShardAccessor
    {
        IndexMeta IndexMeta { get; }
        VectorDto ZeroVector { get; }
        long DataPointsCount { get; }
        ISearchQueryExecutor SearchQueryExecutor { get; }
    }
}
