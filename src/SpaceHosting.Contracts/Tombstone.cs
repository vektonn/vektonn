using System.Collections.Generic;

namespace SpaceHosting.Contracts
{
    public record Tombstone(Dictionary<string, AttributeValue> IdAttributes);
}
