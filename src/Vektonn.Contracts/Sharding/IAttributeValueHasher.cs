namespace Vektonn.Contracts.Sharding
{
    public interface IAttributeValueHasher
    {
        ulong ComputeHash(AttributeValue attributeValue);
    }
}
