using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using AutoFixture;
using FluentAssertions;
using Kongverge.DTOs;
using Kongverge.Services;
using Moq;
using Moq.Language.Flow;
using Newtonsoft.Json;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;

namespace Kongverge.Tests.Services
{
    [Story(Title = nameof(KongAdminReader))]
    public class KongAdminReaderTests : ScenarioFor<KongAdminReader>
    {
        protected const string KongHasPagedDataStepTextTemplate = "Kong has paged data at {0} with <MultiplePages>";
        protected object Result;
        protected object Expected;

        protected bool MultiplePages = default;

        public KongAdminReaderTests()
        {
            GetMock<FakeHttpMessageHandler>().CallBase = true;
            Use(new KongAdminHttpClient(Get<FakeHttpMessageHandler>()) { BaseAddress = new Uri("http://localhost") });
        }

        [BddfyFact(DisplayName = nameof(KongAdminReader.GetConfiguration))]
        public void Scenario1() =>
            this.Given(s => s.KongHasData("/", () => Fixture.Create<KongConfiguration>()))
                .When(async () => Result = await Subject.GetConfiguration(), Invoking(nameof(KongAdminReader.GetConfiguration)))
                .Then(s => s.TheResultIsEquivalentToTheKongData())
                .BDDfy();

        [BddfyFact(DisplayName = nameof(KongAdminReader.GetServices))]
        public void Scenario2() =>
            this.Given(s => s.KongHasPagedData("/services", () => Fixture.Build<KongService>().Without(x => x.Plugins).Without(x => x.Routes).Create()), KongHasPagedDataStepTextTemplate)
                .When(async () => Result = await Subject.GetServices(), Invoking(nameof(KongAdminReader.GetServices)))
                .Then(s => s.TheResultIsEquivalentToTheKongData())
                .WithExamples(new ExampleTable(nameof(MultiplePages)) { false, true })
                .BDDfy();

        [BddfyFact(DisplayName = nameof(KongAdminReader.GetRoutes))]
        public void Scenario3() =>
            this.Given(s => s.KongHasPagedData("/routes", () => Fixture.Build<KongRoute>().Without(x => x.Plugins).Create()), KongHasPagedDataStepTextTemplate)
                .When(async () => Result = await Subject.GetRoutes(), Invoking(nameof(KongAdminReader.GetRoutes)))
                .Then(s => s.TheResultIsEquivalentToTheKongData())
                .WithExamples(new ExampleTable(nameof(MultiplePages)) { false, true })
                .BDDfy();

        [BddfyFact(DisplayName = nameof(KongAdminReader.GetPlugins))]
        public void Scenario4() =>
            this.Given(s => s.KongHasPagedData("/plugins", () => Fixture.Create<KongPlugin>()), KongHasPagedDataStepTextTemplate)
                .When(async () => Result = await Subject.GetPlugins(), Invoking(nameof(KongAdminReader.GetPlugins)))
                .Then(s => s.TheResultIsEquivalentToTheKongData())
                .WithExamples(new ExampleTable(nameof(MultiplePages)) { false, true })
                .BDDfy();

        [BddfyFact(DisplayName = nameof(KongAdminReader.GetPluginSchema))]
        public void Scenario5() =>
            this.Given(s => s.KongHasData("/plugins/schema/test-plugin", () => new KongPluginSchema()))
                .When(async () => Result = await Subject.GetPluginSchema("test-plugin"), Invoking(nameof(KongAdminReader.GetPluginSchema)))
                .Then(s => s.TheResultIsEquivalentToTheKongData())
                .BDDfy();

        protected void KongHasPagedData<T>(string route, Func<T> makeObject)
        {
            var data = Enumerable.Range(0, 3).Select(x => makeObject()).ToArray();
            if (MultiplePages)
            {
                SetupKongResponse($"{route}").Returns(OkResponse(Paged(new[] { data[0] }, $"{route}?page=2")));
                SetupKongResponse($"{route}?page=2").Returns(OkResponse(Paged(new[] { data[1] }, $"{route}?page=3")));
                SetupKongResponse($"{route}?page=3").Returns(OkResponse(Paged(new[] { data[2] })));
            }
            else
            {
                SetupKongResponse($"{route}").Returns(OkResponse(Paged(data)));
            }

            Expected = data;
        }

        protected void KongHasData<T>(string route, Func<T> makeObject)
        {
            Expected = makeObject();
            SetupKongResponse(route).Returns(OkResponse(Expected));
        }

        protected PagedResponse<T> Paged<T>(T[] data, string next = null)
        {
            return new PagedResponse<T>
            {
                Data = data,
                Next = next
            };
        }

        protected string Invoking(string name) => $"invoking {name}";

        protected void TheResultIsEquivalentToTheKongData() => Result.Should().BeEquivalentTo(Expected);

        protected ISetup<FakeHttpMessageHandler, HttpResponseMessage> SetupKongResponse(string pathAndQuery) =>
            GetMock<FakeHttpMessageHandler>()
                .Setup(x => x.Send(It.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Get && m.RequestUri.PathAndQuery == pathAndQuery)));

        protected HttpResponseMessage OkResponse(object content)
        {
            var message = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(content))
            };
            return message;
        }

        public enum KongConnection
        {
            NotReachable,
            Poorly,
            Healthy
        }
    }
}
