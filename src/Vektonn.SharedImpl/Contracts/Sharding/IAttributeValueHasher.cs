namespace Vektonn.SharedImpl.Contracts.Sharding
{
    public interface IAttributeValueHasher
    {
        ulong ComputeHash(AttributeValue attributeValue);
    }
}
