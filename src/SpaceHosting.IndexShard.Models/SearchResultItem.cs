using SpaceHosting.Index;

namespace SpaceHosting.IndexShard.Models
{
    public record SearchResultItem<TVector>(TVector QueryVector, FoundDataPoint<TVector>[] NearestDataPoints)
        where TVector : IVector;
}
