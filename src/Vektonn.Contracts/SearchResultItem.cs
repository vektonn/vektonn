using Vektonn.Index;

namespace Vektonn.Contracts
{
    public record SearchResultItem<TVector>(TVector QueryVector, FoundDataPoint<TVector>[] NearestDataPoints)
        where TVector : IVector;
}
