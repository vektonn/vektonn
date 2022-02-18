using System;

namespace Vektonn.ApiClient.HttpClusterClient
{
    internal interface IRequestUrlBuilder
    {
        Uri BuildRequestUrl(string path);
    }
}
