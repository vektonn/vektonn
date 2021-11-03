using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Vektonn.Index;

namespace Vektonn.SharedImpl.Contracts
{
    public record DataPointOrTombstone<TVector>(DataPoint<TVector>? DataPoint, Tombstone? Tombstone)
        where TVector : IVector
    {
        [SuppressMessage("ReSharper", "IntroduceOptionalParameters.Global")]
        public DataPointOrTombstone(DataPoint<TVector> dataPoint)
            : this(dataPoint, Tombstone: null)
        {
        }

        public DataPointOrTombstone(Tombstone tombstone)
            : this(DataPoint: null, tombstone)
        {
        }

        public Dictionary<string, AttributeValue> GetAttributes()
        {
            if (DataPoint != null)
                return DataPoint.Attributes;

            if (Tombstone != null)
                return Tombstone.IdAttributes;

            throw new InvalidOperationException($"{nameof(DataPoint)} == null && {nameof(Tombstone)} == null");
        }
    }
}
