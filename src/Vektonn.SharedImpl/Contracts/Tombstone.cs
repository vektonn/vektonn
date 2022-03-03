using System.Collections.Generic;

namespace Vektonn.SharedImpl.Contracts
{
    public record Tombstone(Dictionary<string, AttributeValue> PermanentAttributes);
}
