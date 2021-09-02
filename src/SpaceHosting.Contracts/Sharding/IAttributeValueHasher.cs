namespace SpaceHosting.Contracts.Sharding
{
    public interface IAttributeValueHasher
    {
        ulong ComputeHash(AttributeValue attributeValue);
    }
}
