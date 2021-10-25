namespace Vektonn.SharedImpl.Contracts.Sharding
{
    public class IdentityAttributeValueProjector : IAttributeValueProjector<AttributeValue>
    {
        public AttributeValue GetProjection(AttributeValue attributeValue)
        {
            return attributeValue;
        }
    }
}
