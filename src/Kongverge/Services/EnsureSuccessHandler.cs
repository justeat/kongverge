using Serilog;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Kongverge.Services
{
    public class EnsureSuccessHandler : DelegatingHandler
    {
        private bool _faultTolerance;
        public EnsureSuccessHandler(bool faultTolerance = false, HttpMessageHandler innerHandler = null) : base(innerHandler ?? new HttpClientHandler())
        {
            _faultTolerance = faultTolerance;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return response;
            }
            
            var responseBody = await response.Content.ReadAsStringAsync();
            if (_faultTolerance)
            {
                Log.Error("Error converging target configuration: StatusCode: {StatusCode}, ResponseBody: {ResponseBody}", response.StatusCode, responseBody);
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent("{}") };
            }
            throw new KongException(response.StatusCode, responseBody);
        }
    }
}
