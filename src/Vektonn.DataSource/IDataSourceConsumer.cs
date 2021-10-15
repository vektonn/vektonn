using System.Threading;
using System.Threading.Tasks;

namespace Vektonn.DataSource
{
    public interface IDataSourceConsumer
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}
