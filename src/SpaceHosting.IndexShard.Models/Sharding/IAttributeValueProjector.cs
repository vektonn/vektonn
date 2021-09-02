namespace SpaceHosting.IndexShard.Models.Sharding
{
    public interface IAttributeValueProjector<out TProjection>
    {
        TProjection GetProjection(AttributeValue attributeValue);
    }
}
