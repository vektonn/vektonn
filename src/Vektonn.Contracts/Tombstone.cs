using System.Collections.Generic;

namespace Vektonn.Contracts
{
    public record Tombstone(Dictionary<string, AttributeValue> IdAttributes);
}
