using Vostok.Hosting.Abstractions;

namespace Vektonn.Hosting
{
    public interface IVektonnApplication : IVostokApplication
    {
        string ApplicationName { get; }
    }
}
