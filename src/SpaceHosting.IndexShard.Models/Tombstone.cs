using System.Collections.Generic;

namespace SpaceHosting.IndexShard.Models
{
    public record Tombstone(Dictionary<string, AttributeValue> IdAttributes);
}
