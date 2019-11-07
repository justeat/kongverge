using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using AutoFixture;
using Kongverge.DTOs;
using Kongverge.Services;
using Moq;
using Moq.Language.Flow;
using Newtonsoft.Json;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;

namespace Kongverge.Tests.Services
{
    [Story(Title = nameof(KongAdminWriter))]
    public class KongAdminWriterTests : ScenarioFor<KongAdminWriter>
    {
        protected const string KongRespondsCorrectlyToMethodAtPathTextTemplate = "And kong responds correctly to {0} at {1}";

        public KongAdminWriterTests()
        {
            GetMock<FakeHttpMessageHandler>().CallBase = true;
            Use(new KongAdminHttpClient(new KongAdminApiConnectionDetails(), Get<FakeHttpMessageHandler>()) { BaseAddress = new Uri("http://localhost") });
        }

        [BddfyFact(DisplayName = nameof(KongAdminWriter.PutConsumer))]
        public void Scenario1()
        {
            KongConsumer consumer = null;

            this.Given(() => consumer = Fixture.Create<KongConsumer>(), "A kong consumer")
                .And(s => s.KongRespondsCorrectly<KongConsumer>(HttpMethod.Put, $"/consumers/{consumer.Id}", consumer.ToJsonStringContent()),
                    KongRespondsCorrectlyToMethodAtPathTextTemplate)
                .When(async () => await Subject.PutConsumer(consumer), Invoking(nameof(KongAdminWriter.PutConsumer)))
                .Then("it succeeds")
                .BDDfy();
        }

        [BddfyFact(DisplayName = nameof(KongAdminWriter.DeleteConsumer))]
        public void Scenario2()
        {
            KongConsumer consumer = null;

            this.Given(() => consumer = Fixture.Create<KongConsumer>(), "A kong consumer")
                .And(s => s.KongRespondsCorrectly(HttpMethod.Delete, $"/consumers/{consumer.Id}"), KongRespondsCorrectlyToMethodAtPathTextTemplate)
                .When(async () => await Subject.DeleteConsumer(consumer.Id), Invoking(nameof(KongAdminWriter.DeleteConsumer)))
                .Then("it succeeds")
                .BDDfy();
        }

        [BddfyFact(DisplayName = nameof(KongAdminWriter.PutService))]
        public void Scenario3()
        {
            KongService service = null;

            this.Given(() => service = Fixture.Create<KongService>(), "A kong service")
                .And(s => s.KongRespondsCorrectly<KongService>(HttpMethod.Put, $"/services/{service.Id}", service.ToJsonStringContent()),
                    KongRespondsCorrectlyToMethodAtPathTextTemplate)
                .When(async () => await Subject.PutService(service), Invoking(nameof(KongAdminWriter.PutService)))
                .Then("it succeeds")
                .BDDfy();
        }

        [BddfyFact(DisplayName = nameof(KongAdminWriter.DeleteService))]
        public void Scenario4()
        {
            KongService service = null;

            this.Given(() =>
                {
                    service = Fixture.Create<KongService>();
                    GetMock<FakeHttpMessageHandler>()
                        .Setup(x => x.Send(It.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Get && m.RequestUri.PathAndQuery == $"/services/{service.Id}/routes")))
                        .Returns(OkResponse(new PagedResponse<KongRoute>
                        {
                            Data = service.Routes.ToArray()
                        }));
                }, "An existing kong service with routes")
                .And(s => s.KongRespondsCorrectly(HttpMethod.Delete, $"/services/{service.Id}"), KongRespondsCorrectlyToMethodAtPathTextTemplate)
                .And(() =>
                {
                    foreach (var serviceRoute in service.Routes)
                    {
                        KongRespondsCorrectly(HttpMethod.Delete, $"/routes/{serviceRoute.Id}");
                    }
                }, "kong responds correctly to deleting service routes")
                .When(async () => await Subject.DeleteService(service.Id), Invoking(nameof(KongAdminWriter.DeleteService)))
                .Then(() =>
                {
                    foreach (var serviceRoute in service.Routes)
                    {
                        GetMock<FakeHttpMessageHandler>()
                            .Verify(x => x.Send(It.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Delete && m.RequestUri.PathAndQuery == $"/routes/{serviceRoute.Id}")));
                    }
                    
                }, "The service routes are deleted first")
                .BDDfy();
        }

        [BddfyFact(DisplayName = nameof(KongAdminWriter.PutRoute))]
        public void Scenario5()
        {
            KongRoute route = null;

            this.Given(() => route = Fixture.Create<KongRoute>(), "A kong route")
                .And(s => s.KongRespondsCorrectly<KongRoute>(HttpMethod.Put, $"/routes/{route.Id}", route.ToJsonStringContent()),
                    KongRespondsCorrectlyToMethodAtPathTextTemplate)
                .When(async () => await Subject.PutRoute(route), Invoking(nameof(KongAdminWriter.PutRoute)))
                .Then("it succeeds")
                .BDDfy();
        }

        [BddfyFact(DisplayName = nameof(KongAdminWriter.DeleteRoute))]
        public void Scenario6()
        {
            KongRoute route = null;

            this.Given(() => route = Fixture.Create<KongRoute>(), "A kong route")
                .And(s => s.KongRespondsCorrectly(HttpMethod.Delete, $"/routes/{route.Id}"), KongRespondsCorrectlyToMethodAtPathTextTemplate)
                .When(async () => await Subject.DeleteRoute(route.Id), Invoking(nameof(KongAdminWriter.DeleteRoute)))
                .Then("it succeeds")
                .BDDfy();
        }

        [BddfyFact(DisplayName = nameof(KongAdminWriter.PutPlugin))]
        public void Scenario7()
        {
            KongPlugin plugin = null;

            this.Given(() => plugin = Fixture.Create<KongPlugin>(), "A kong plugin")
                .And(s => s.KongRespondsCorrectly<KongPlugin>(HttpMethod.Put, $"/plugins/{plugin.Id}", plugin.ToJsonStringContent()),
                    KongRespondsCorrectlyToMethodAtPathTextTemplate)
                .When(async () => await Subject.PutPlugin(plugin), Invoking(nameof(KongAdminWriter.PutPlugin)))
                .Then("it succeeds")
                .BDDfy();
        }

        [BddfyFact(DisplayName = nameof(KongAdminWriter.DeletePlugin))]
        public void Scenario8()
        {
            KongPlugin plugin = null;

            this.Given(() => plugin = Fixture.Create<KongPlugin>(), "A kong plugin")
                .And(s => s.KongRespondsCorrectly(HttpMethod.Delete, $"/plugins/{plugin.Id}"), KongRespondsCorrectlyToMethodAtPathTextTemplate)
                .When(async () => await Subject.DeletePlugin(plugin.Id), Invoking(nameof(KongAdminWriter.DeletePlugin)))
                .Then("it succeeds")
                .BDDfy();
        }

        protected string Invoking(string name) => $"invoking {name}";

        protected void KongRespondsCorrectly<T>(HttpMethod httpMethod, string route, StringContent content)
        {
            SetupKongResponse(httpMethod, route, content).Returns(OkResponse());
        }

        protected void KongRespondsCorrectly(HttpMethod httpMethod, string route)
        {
            SetupKongResponse(httpMethod, route, null).Returns(OkResponse());
        }

        protected ISetup<FakeHttpMessageHandler, HttpResponseMessage> SetupKongResponse(HttpMethod httpMethod, string pathAndQuery, StringContent content) =>
            GetMock<FakeHttpMessageHandler>()
                .Setup(x => x.Send(It.Is<HttpRequestMessage>(m => m.Method == httpMethod && m.RequestUri.PathAndQuery == pathAndQuery && StringContentMatches(m.Content, content))));

        private static bool StringContentMatches(HttpContent actual, StringContent expected)
        {
            if (expected == null)
            {
                return actual == null;
            }
            if (actual is StringContent stringContent)
            {
                var actualData = stringContent.ReadAsStringAsync().Result;
                var expectedData = expected.ReadAsStringAsync().Result;
                return actualData == expectedData;
            }
            return false;
        }

        protected HttpResponseMessage OkResponse(object content = null)
        {
            var message = new HttpResponseMessage(HttpStatusCode.OK);
            if (content != null)
            {
                message.Content = new StringContent(JsonConvert.SerializeObject(content));
            }
            return message;
        }
    }
}
