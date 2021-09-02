namespace SpaceHosting.Contracts.Sharding.DataSource
{
    public interface IDataSourceAttributeValueSharder
    {
        bool IsValueAcceptable(AttributeValue attributeValue);
    }
}
