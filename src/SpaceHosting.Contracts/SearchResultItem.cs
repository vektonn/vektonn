using SpaceHosting.Index;

namespace SpaceHosting.Contracts
{
    public record SearchResultItem<TVector>(TVector QueryVector, FoundDataPoint<TVector>[] NearestDataPoints)
        where TVector : IVector;
}
