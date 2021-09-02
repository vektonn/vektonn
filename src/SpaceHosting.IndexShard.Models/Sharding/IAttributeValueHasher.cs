namespace SpaceHosting.IndexShard.Models.Sharding
{
    public interface IAttributeValueHasher
    {
        ulong ComputeHash(AttributeValue attributeValue);
    }
}
