using SpaceHosting.ApiModels;

namespace SpaceHosting.Service.IndexStore
{
    public interface IIndexStoreAccessor
    {
        string IndexAlgorithm { get; }
        int VectorDimension { get; }
        int VectorCount { get; }
        VectorDto ZeroVector { get; }
        SearchResultDto[][] Search(SearchQueryDto searchQuery);
    }
}
