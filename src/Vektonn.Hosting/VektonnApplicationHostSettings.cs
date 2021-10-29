using System;

namespace Vektonn.Hosting
{
    public record VektonnApplicationHostSettings(int Port, string HostingEnvironment)
    {
        public TimeSpan ShutdownTimeout { get; } = TimeSpan.FromSeconds(10);

        public static VektonnApplicationHostSettings FromEnvironmentVariables()
        {
            var httpPort = EnvironmentVariables.Get("VEKTONN_HTTP_PORT", int.Parse);
            var hostingEnvironment = EnvironmentVariables.Get("VEKTONN_HOSTING_ENVIRONMENT");
            return new VektonnApplicationHostSettings(httpPort, hostingEnvironment);
        }
    }
}
