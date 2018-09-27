using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using AutoFixture;
using FluentAssertions;
using Kongverge.DTOs;
using Kongverge.Services;
using Kongverge.Tests.Workflow;
using Moq;
using Moq.Language.Flow;
using Newtonsoft.Json;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;

namespace Kongverge.Tests.Services
{
    public class KongAdminWriterTests : ScenarioFor<KongAdminWriter>
    {
        protected const string KongRespondsCorrectlyToMethodAtPathTextTemplate = "And kong responds correctly to {0} at {1}";

        public KongAdminWriterTests()
        {
            GetMock<FakeHttpMessageHandler>().CallBase = true;
            Use(new KongAdminHttpClient(Get<FakeHttpMessageHandler>()) { BaseAddress = new Uri("http://localhost") });
        }

        [BddfyFact(DisplayName = nameof(KongAdminWriter.AddService))]
        public void Scenario1()
        {
            KongService service = null;

            this.Given(() => service = Fixture.Build<KongService>().Without(x => x.Id).Create(), "A new kong service")
                .And(s => s.KongRespondsCorrectly<KongService>(HttpMethod.Post, "/services/", service.ToJsonStringContent(), x => x.WithIdAndCreatedAt()),
                    KongRespondsCorrectlyToMethodAtPathTextTemplate)
                .When(async () => await Subject.AddService(service), Invoking(nameof(KongAdminWriter.AddService)))
                .Then(() => service.Id.Should().NotBeNullOrWhiteSpace(), "the Id is set")
                .BDDfy();
        }

        [BddfyFact(DisplayName = nameof(KongAdminWriter.UpdateService))]
        public void Scenario2()
        {
            KongService service = null;

            this.Given(() => service = Fixture.Create<KongService>(), "An existing kong service")
                .And(s => s.KongRespondsCorrectly<KongService>(new HttpMethod("PATCH"), $"/services/{service.Id}", service.ToJsonStringContent(), x => x.WithIdAndCreatedAt()),
                    KongRespondsCorrectlyToMethodAtPathTextTemplate)
                .When(async () => await Subject.UpdateService(service), Invoking(nameof(KongAdminWriter.UpdateService)))
                .Then("it succeeds")
                .BDDfy();
        }

        [BddfyFact(DisplayName = nameof(KongAdminWriter.DeleteService))]
        public void Scenario3()
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

        [BddfyFact(DisplayName = nameof(KongAdminWriter.AddRoute))]
        public void Scenario4()
        {
            string serviceId = Guid.NewGuid().ToString();
            KongRoute route = null;

            this.Given(() => route = Fixture.Build<KongRoute>().Without(x => x.Id).Create(), "A new kong route")
                .And(s => s.KongRespondsCorrectly<KongRoute>(HttpMethod.Post, $"/services/{serviceId}/routes", route.ToJsonStringContent(), x => x.WithIdAndCreatedAtAndServiceReference(serviceId)),
                    KongRespondsCorrectlyToMethodAtPathTextTemplate)
                .When(async () => await Subject.AddRoute(serviceId, route), Invoking(nameof(KongAdminWriter.AddRoute)))
                .Then(() => route.Id.Should().NotBeNullOrWhiteSpace(), "the Id is set")
                .And(() => route.Service.Id.Should().Be(serviceId), "the ServiceReference is set")
                .BDDfy();
        }

        [BddfyFact(DisplayName = nameof(KongAdminWriter.DeleteRoute))]
        public void Scenario5()
        {
            KongRoute route = null;

            this.Given(() => route = Fixture.Create<KongRoute>(), "An existing kong route")
                .And(s => s.KongRespondsCorrectly(HttpMethod.Delete, $"/routes/{route.Id}"), KongRespondsCorrectlyToMethodAtPathTextTemplate)
                .When(async () => await Subject.DeleteRoute(route.Id), Invoking(nameof(KongAdminWriter.DeleteRoute)))
                .Then("it succeeds")
                .BDDfy();
        }

        [BddfyFact(DisplayName = nameof(KongAdminWriter.UpsertPlugin))]
        public void Scenario6()
        {
            KongPlugin plugin = null;

            this.Given(() => plugin = Fixture.Build<KongPlugin>().Without(x => x.Id).Create(), "A new plugin")
                .And(s => s.KongRespondsCorrectly<KongService>(HttpMethod.Put, "/plugins", plugin.ToJsonStringContent(), x => x.WithIdAndCreatedAt()),
                    KongRespondsCorrectlyToMethodAtPathTextTemplate)
                .When(async () => await Subject.UpsertPlugin(plugin), Invoking(nameof(KongAdminWriter.UpsertPlugin)))
                .Then(() => plugin.Id.Should().NotBeNullOrWhiteSpace(), "the Id is set")
                .BDDfy();
        }

        [BddfyFact(DisplayName = nameof(KongAdminWriter.DeletePlugin))]
        public void Scenario7()
        {
            KongPlugin plugin = null;

            this.Given(() => plugin = Fixture.Create<KongPlugin>(), "An existing kong plugin")
                .And(s => s.KongRespondsCorrectly(HttpMethod.Delete, $"/plugins/{plugin.Id}"), KongRespondsCorrectlyToMethodAtPathTextTemplate)
                .When(async () => await Subject.DeletePlugin(plugin.Id), Invoking(nameof(KongAdminWriter.DeletePlugin)))
                .Then("it succeeds")
                .BDDfy();
        }

        protected string Invoking(string name) => $"invoking {name}";

        protected void KongRespondsCorrectly<T>(HttpMethod httpMethod, string route, StringContent content, Action<T> kongAction)
        {
            SetupKongResponse(httpMethod, route, content).Returns(() =>
            {
                var data = content.ReadAsStringAsync().Result;
                var responseObject = JsonConvert.DeserializeObject<T>(data);
                kongAction(responseObject);
                return OkResponse(responseObject);
            });
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
