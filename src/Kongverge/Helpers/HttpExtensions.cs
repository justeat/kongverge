using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kongverge.Helpers
{
    public static class HttpExtensions
    {
        public static StringContent AsJsonStringContent(this string json) =>
            new StringContent(json, Encoding.UTF8, "application/json");

        public static Task<HttpResponseMessage> PatchAsync(
            this HttpClient client,
            string requestUri,
            HttpContent content,
            CancellationToken cancellationToken = default) =>
            client.PatchAsync(new Uri(requestUri, UriKind.RelativeOrAbsolute), content, cancellationToken);

        public static Task<HttpResponseMessage> PatchAsync(
            this HttpClient client,
            Uri requestUri,
            HttpContent content,
            CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri) { Content = content };
            return client.SendAsync(request, cancellationToken);
        }
    }
}
