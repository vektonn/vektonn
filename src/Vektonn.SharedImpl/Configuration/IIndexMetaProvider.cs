using Vektonn.SharedImpl.Contracts;

namespace Vektonn.SharedImpl.Configuration
{
    public interface IIndexMetaProvider
    {
        IndexMeta? TryGetIndexMeta(IndexId indexId);
        DataSourceMeta? TryGetDataSourceMeta(DataSourceId dataSourceId);
    }
}
