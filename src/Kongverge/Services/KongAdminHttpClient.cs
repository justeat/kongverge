using System;
using System.Net.Http;
using Kongverge.DTOs;
using Microsoft.Extensions.Options;

namespace Kongverge.Services
{
    public class KongAdminHttpClient : HttpClient
    {
        public KongAdminHttpClient(IOptions<Settings> configuration) : base(new EnsureSuccessHandler())
        {
            BaseAddress = new Uri($"http://{configuration.Value.Admin.Host}:{configuration.Value.Admin.Port}");
        }

        public KongAdminHttpClient(HttpMessageHandler innerHandler) : base(new EnsureSuccessHandler(innerHandler)) { }
    }
}
