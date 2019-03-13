using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Kongverge.IntegrationTests
{
    public class FakeHttpMessageHandler : HttpClientHandler
    {
        private readonly Func<FakeHttpMessageHandler, HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsyncImplementation;

        public FakeHttpMessageHandler(Func<FakeHttpMessageHandler, HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsyncImplementation) => _sendAsyncImplementation = sendAsyncImplementation;

        public Task<HttpResponseMessage> NormalSendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => base.SendAsync(request, cancellationToken);

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => _sendAsyncImplementation(this, request, cancellationToken);
    }
}
