namespace SpaceHosting.IndexShard.Models.Sharding
{
    public class IdentityAttributeValueProjector : IAttributeValueProjector<AttributeValue>
    {
        public AttributeValue GetProjection(AttributeValue attributeValue)
        {
            return attributeValue;
        }
    }
}
