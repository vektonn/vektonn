using SpaceHosting.ApiModels;
using SpaceHosting.Index;

namespace SpaceHosting.Service.IndexStore
{
    public interface IIndexStoreAccessor
    {
        string IndexAlgorithm { get; }
        int VectorDimension { get; }
        int VectorCount { get; }
        IVector ZeroVector { get; }
        SearchResultDto[][] Search(SearchQueryDto searchQuery);
    }
}
