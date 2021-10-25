using System.Collections.Generic;
using System.Threading.Tasks;
using Vektonn.SharedImpl.Contracts;

namespace Vektonn.DataSource
{
    public interface IDataSourceProducer
    {
        Task ProduceAsync(DataSourceDescriptor dataSource, IReadOnlyList<InputDataPointOrTombstone> dataPointOrTombstones);
    }
}
