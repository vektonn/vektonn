using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Vektonn.SharedImpl.Contracts
{
    public record InputDataPointOrTombstone(InputDataPoint? DataPoint, Tombstone? Tombstone)
    {
        [SuppressMessage("ReSharper", "IntroduceOptionalParameters.Global")]
        public InputDataPointOrTombstone(InputDataPoint dataPoint)
            : this(dataPoint, Tombstone: null)
        {
        }

        public InputDataPointOrTombstone(Tombstone tombstone)
            : this(DataPoint: null, tombstone)
        {
        }

        public Dictionary<string, AttributeValue> GetAttributes()
        {
            if (DataPoint != null)
                return DataPoint.Attributes;

            if (Tombstone != null)
                return Tombstone.PermanentAttributes;

            throw new InvalidOperationException($"{nameof(DataPoint)} == null && {nameof(Tombstone)} == null");
        }
    }
}
