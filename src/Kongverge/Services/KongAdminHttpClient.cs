using Kongverge.DTOs;
using System;
using System.Net.Http;

namespace Kongverge.Services
{
    public class KongAdminHttpClient : HttpClient
    {
        public KongAdminHttpClient(KongAdminApiConnectionDetails connectionDetails, HttpMessageHandler innerHandler = null, KongvergeWorkflowArguments kongvergeWorkflowArguments = null) : base(new EnsureSuccessHandler(kongvergeWorkflowArguments.FaultTolerance, innerHandler))
        {
            BaseAddress = new Uri($"http://{connectionDetails.Host}:{connectionDetails.Port}");
            DefaultRequestHeaders.Authorization = connectionDetails.AuthenticationHeader;
        }
    }
}
