using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Kongverge.Services
{
    public class EnsureSuccessHandler : DelegatingHandler
    {
        public EnsureSuccessHandler() : base(new HttpClientHandler()) { }

        public EnsureSuccessHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return response;
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            throw new KongException(response.StatusCode, responseBody);
        }
    }
}
