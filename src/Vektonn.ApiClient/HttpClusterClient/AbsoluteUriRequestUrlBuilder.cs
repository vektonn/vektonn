using System;

namespace Vektonn.ApiClient.HttpClusterClient
{
    internal class AbsoluteUriRequestUrlBuilder : IRequestUrlBuilder
    {
        private readonly Uri baseUri;

        public AbsoluteUriRequestUrlBuilder(Uri baseUri)
        {
            if (!baseUri.IsAbsoluteUri)
                throw new InvalidOperationException($"{nameof(baseUri)} does not represent an absolute Uri: {baseUri}");

            this.baseUri = baseUri;
        }

        public Uri BuildRequestUrl(string path) => new Uri(baseUri, path);
    }
}
