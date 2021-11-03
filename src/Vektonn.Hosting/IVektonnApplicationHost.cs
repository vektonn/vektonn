using System.Threading.Tasks;

namespace Vektonn.Hosting
{
    public interface IVektonnApplicationHost
    {
        Task StartAsync();
        Task StopAsync();
        Task RunToCompletionAsync();
    }
}
