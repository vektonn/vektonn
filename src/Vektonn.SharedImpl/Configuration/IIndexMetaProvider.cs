using Vektonn.SharedImpl.Contracts;

namespace Vektonn.SharedImpl.Configuration
{
    public interface IIndexMetaProvider
    {
        IndexMetaWithShardEndpoints? TryGetIndexMeta(IndexId indexId);
        DataSourceMeta? TryGetDataSourceMeta(DataSourceId dataSourceId);
    }
}
