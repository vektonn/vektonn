using System.Collections.Generic;

namespace Vektonn.SharedImpl.Contracts
{
    public record InputDataPoint(
        Dictionary<string, AttributeValue> Attributes,
        double[] VectorCoordinates,
        int[]? VectorCoordinateIndices);
}
