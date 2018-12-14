using System;
using System.Net.Http;
using Kongverge.DTOs;

namespace Kongverge.Services
{
    public class KongAdminHttpClient : HttpClient
    {
        public KongAdminHttpClient(KongAdminApiConnectionDetails connectionDetails) : base(new EnsureSuccessHandler())
        {
            BaseAddress = new Uri($"http://{connectionDetails.Host}:{connectionDetails.Port}");
            DefaultRequestHeaders.Authorization = connectionDetails.AuthenticationHeader;
        }

        public KongAdminHttpClient(HttpMessageHandler innerHandler) : base(new EnsureSuccessHandler(innerHandler)) { }
    }
}
