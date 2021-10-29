using System;
using Vektonn.DataSource;

namespace Vektonn.IndexShardService.Services
{
    public interface IIndexShardOwner : IDisposable, IIndexShardAccessor
    {
        IDataSourceConsumer DataSourceConsumer { get; }
    }
}
