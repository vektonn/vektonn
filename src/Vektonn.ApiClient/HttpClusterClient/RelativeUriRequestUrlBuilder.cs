using System;

namespace Vektonn.ApiClient.HttpClusterClient
{
    internal class RelativeUriRequestUrlBuilder : IRequestUrlBuilder
    {
        public Uri BuildRequestUrl(string path) => new Uri(path, UriKind.Relative);
    }
}
