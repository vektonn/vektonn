using SpaceHosting.IndexShard.Models;

namespace SpaceHosting.IndexShard.Tests.IndexShard
{
    public static class AttributeValueTestHelpers
    {
        public static AttributeValue AttributeValue(long value)
        {
            return new AttributeValue(Int64: value);
        }

        public static AttributeValue AttributeValue(string value)
        {
            return new AttributeValue(String: value);
        }

        public static AttributeValue AttributeValue(bool value)
        {
            return new AttributeValue(Bool: value);
        }
    }
}
