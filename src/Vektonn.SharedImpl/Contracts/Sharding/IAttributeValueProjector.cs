namespace Vektonn.SharedImpl.Contracts.Sharding
{
    public interface IAttributeValueProjector<out TProjection>
    {
        TProjection GetProjection(AttributeValue attributeValue);
    }
}
