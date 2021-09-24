namespace Vektonn.Contracts.Sharding
{
    public interface IAttributeValueProjector<out TProjection>
    {
        TProjection GetProjection(AttributeValue attributeValue);
    }
}
