using AutoFixture;
using AutoFixture.Dsl;
using FluentAssertions;
using Kongverge.DTOs;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;

namespace Kongverge.Tests.DTOs
{
    [Story(Title = nameof(KongService) + nameof(Equals))]
    public class KongServiceEqualityScenarios : EqualityScenarios<KongService>
    {
        protected override void OnlyThePersistenceValuesAreDifferent()
        {
            OtherInstance.Id = this.Create<string>();
            OtherInstance.CreatedAt = this.Create<long>();
        }
    }

    [Story(Title = nameof(KongService) + nameof(IValidatableObject.Validate))]
    public class KongServiceValidationScenarios : ValidatableObjectSteps<KongService>
    {
        protected Children Plugins;
        protected Children Routes;
        protected string Name;
        protected string Protocol;
        protected string Host;
        protected string Path;

        [BddfyFact(DisplayName = nameof(AValidInstanceWithExamplePluginsAndRoutes))]
        public void Scenario1() =>
            this.Given(x => x.AValidInstanceWithExamplePluginsAndRoutes())
                .When(x => x.Validating())
                .Then(x => x.TheErrorMessagesCountIs(ErrorMessagesCount))
                .WithExamples(new ExampleTable(nameof(Plugins), nameof(Routes), nameof(ErrorMessagesCount))
                {
                    { Children.Null,     Children.Null,     2 },
                    { Children.Empty,    Children.Empty,    1 },
                    { Children.Valid,    Children.Valid,    0 },
                    { Children.OneError, Children.Valid,    1 },
                    { Children.Valid,    Children.OneError, 1 },
                    { Children.OneError, Children.OneError, 2 }
                })
                .BDDfy();

        [BddfyFact(DisplayName = nameof(AnInstanceWithValidRoutesAndExamplePropertyValues))]
        public void Scenario2() =>
            this.Given(x => x.AnInstanceWithValidRoutesAndExamplePropertyValues())
                .When(x => x.Validating())
                .Then(x => x.TheErrorMessagesCountIs(ErrorMessagesCount))
                .WithExamples(new ExampleTable(
                    nameof(Name), nameof(Protocol), nameof(Host), nameof(Path), nameof(Tags), nameof(ErrorMessagesCount))
                {
                    { "name",             "http",   "localhost",                               "foo",                    Set.Null,       1 },
                    { "name",             "http",   "www.example.com",                         "/foo",                   Set.Null,       0 },
                    { "name",             "junk",   "2001:0db8:85a3:0000:0000:8a2e:0370:7334", "/foo/bar",               Set.Null,       1 },
                    { "name",             "https",  null,                                      "/foo/bar/baz",           Set.Null,       1 },
                    { "name",             "https",  "127.0.0.1",                               "",                       Set.Null,       2 },
                    { null,               "http",   ":",                                       null,                     Set.InvalidSet, 1 }
                })
                .BDDfy();

        protected void AValidInstanceWithExamplePluginsAndRoutes() => Instance = BuildServiceWithoutPluginsOrRoutes(this)
            .With(x => x.Plugins, this.CreateChildren(Plugins, this.GetValidKongPlugin, this.GetKongPluginWithOneError))
            .With(x => x.Routes, this.CreateChildren(Routes, this.GetValidKongRoute, this.GetKongRouteWithOneError))
            .Create();

        protected void AnInstanceWithValidRoutesAndExamplePropertyValues() => Instance = BuildServiceWithoutPluginsOrRoutes(
                this, Name, Protocol, Host, Path, tags:Tags)
            .Create();

        public static IPostprocessComposer<KongService> BuildServiceWithoutPluginsOrRoutes(
            Fixture fixture,
            string name = "name",
            string protocol = "https",
            string host = "foo.bar.baz",
            string path = null,
            byte retries = 5,
            uint connectTimeout = 1000U,
            uint writeTimeout = 1000U,
            uint readTimeout = 1000U,
            Set tags = Set.ValidItems) => fixture.Build<KongService>()
            .With(x => x.Id, fixture.Create<string>())
            .With(x => x.Plugins, fixture.CreateChildren(Children.Empty, fixture.GetValidKongPlugin, fixture.GetKongPluginWithOneError))
            .With(x => x.Routes, fixture.CreateChildren(Children.Valid, fixture.GetValidKongRoute, fixture.GetKongRouteWithOneError))
            .With(x => x.Name, name)
            .With(x => x.Protocol, protocol)
            .With(x => x.Host, host)
            .With(x => x.Path, path)
            .With(x => x.Retries, retries)
            .With(x => x.ConnectTimeout, connectTimeout)
            .With(x => x.WriteTimeout, writeTimeout)
            .With(x => x.ReadTimeout, readTimeout)
            .With(x => x.Tags, fixture.CreateTags(tags));
    }

    [Story(Title = nameof(KongService) + nameof(KongObject.ToJsonStringContent))]
    public class KongServiceSerializationScenarios : KongObjectSerializationScenarios<KongService>
    {
        [BddfyFact(DisplayName = nameof(ARandomInstance) + And + nameof(SerializingToStringContent))]
        public void Scenario1() =>
            this.Given(x => x.ARandomInstance())
                .When(x => x.SerializingToStringContent())
                .Then(x => x.ClientCertificateIsSerialized())
                .And(x => x.RoutesIsNotSerialized())
                .And(x => x.PluginsIsNotSerialized())
                .And(x => x.ClientCertificateIsNotNull())
                .And(x => x.RoutesIsNotNull())
                .And(x => x.PluginsIsNotNull())
                .BDDfy();

        protected void ClientCertificateIsSerialized() => Serialized.Should().Contain("\"client_certificate\":");

        protected void RoutesIsNotSerialized() => Serialized.Should().NotContain("\"routes\":");

        protected void PluginsIsNotSerialized() => Serialized.Should().NotContain("\"plugins\":");

        protected void ClientCertificateIsNotNull() => Instance.ClientCertificate.Should().NotBeNull();

        protected void RoutesIsNotNull() => Instance.Routes.Should().NotBeNull();

        protected void PluginsIsNotNull() => Instance.Plugins.Should().NotBeNull();
    }

    [Story(Title = nameof(KongService) + nameof(IKongvergeConfigObject.ToConfigJson))]
    public class KongServiceConfigSerializationScenarios : KongvergeConfigObjectSerializationScenarios<KongService> { }
}
