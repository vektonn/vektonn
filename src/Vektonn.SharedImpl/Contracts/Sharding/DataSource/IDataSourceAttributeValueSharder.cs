namespace Vektonn.SharedImpl.Contracts.Sharding.DataSource
{
    public interface IDataSourceAttributeValueSharder
    {
        // todo (andrew, 11.09.2021): remove when DataPoints upload with any attribute values will be allowed
        bool IsValueAcceptable(AttributeValue attributeValue);

        ulong GetShardingCoordinate(AttributeValue attributeValue);
    }
}
