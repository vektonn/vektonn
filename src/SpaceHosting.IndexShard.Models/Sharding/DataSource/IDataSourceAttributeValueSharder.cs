namespace SpaceHosting.IndexShard.Models.Sharding.DataSource
{
    public interface IDataSourceAttributeValueSharder
    {
        bool IsValueAcceptable(AttributeValue attributeValue);
    }
}
