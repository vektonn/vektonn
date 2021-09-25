using System.Collections.Generic;

namespace Vektonn.Contracts
{
    public record InputDataPoint(
        Dictionary<string, AttributeValue> Attributes,
        double[] VectorCoordinates,
        int[]? VectorCoordinateIndices);
}
