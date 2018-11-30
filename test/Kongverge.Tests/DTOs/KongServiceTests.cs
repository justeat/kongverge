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
        protected CollectionExample Plugins;
        protected CollectionExample Routes;
        protected string Protocol;
        protected string Host;
        protected string Path;
        protected byte Retries;
        protected uint ConnectTimeout;
        protected uint WriteTimeout;
        protected uint ReadTimeout;

        [BddfyFact(DisplayName = nameof(AValidInstanceWithExamplePluginsAndRoutes))]
        public void Scenario1() =>
            this.Given(x => x.AValidInstanceWithExamplePluginsAndRoutes())
                .When(x => x.Validating())
                .Then(x => x.TheErrorMessagesCountIs(ErrorMessagesCount))
                .WithExamples(new ExampleTable(nameof(Plugins), nameof(Routes), nameof(ErrorMessagesCount))
                {
                    { CollectionExample.Null, CollectionExample.Null, 2 },
                    { CollectionExample.Empty, CollectionExample.Empty, 1 },
                    { CollectionExample.Valid, CollectionExample.Valid, 0 },
                    { CollectionExample.Invalid, CollectionExample.Valid, 1 },
                    { CollectionExample.Valid, CollectionExample.Invalid, 1 },
                    { CollectionExample.Invalid, CollectionExample.Invalid, 2 }
                })
                .BDDfy();

        [BddfyFact(DisplayName = nameof(AnInstanceWithValidRoutesAndExamplePropertyValues))]
        public void Scenario2() =>
            this.Given(x => x.AnInstanceWithValidRoutesAndExamplePropertyValues())
                .When(x => x.Validating())
                .Then(x => x.TheErrorMessagesCountIs(ErrorMessagesCount))
                .WithExamples(new ExampleTable(nameof(Protocol), nameof(Host), nameof(Path), nameof(Retries), nameof(ConnectTimeout), nameof(WriteTimeout), nameof(ReadTimeout), nameof(ErrorMessagesCount))
                {
                    { "http", "localhost", "path", 0, 0, 0, 0, 0 },
                    { "http", "www.example.com", "path/sub-path", 0, 0, 0, 0, 0 },
                    { "http", "2001:0db8:85a3:0000:0000:8a2e:0370:7334", "path/sub-path/sub-sub-path", 0, 0, 0, 0, 0 },
                    { "https", "127.0.0.1", null, 25, 300000, 300000, 300000, 0 },
                    { "https", null, null, 25, 300000, 300000, 300000, 1 },
                    { "junk", ":", "path:invalid", 26, 300001, 300001, 300001, 7 }
                })
                .BDDfy();

        protected void AValidInstanceWithExamplePluginsAndRoutes() => Instance = BuildValidService()
            .With(x => x.Plugins, GetExampleCollection<KongPlugin>(Plugins))
            .With(x => x.Routes, GetExampleCollection<KongRoute>(Routes))
            .Create();

        protected void AnInstanceWithValidRoutesAndExamplePropertyValues() => Instance = Build<KongService>()
            .With(x => x.Plugins, GetExampleCollection<KongPlugin>(CollectionExample.Empty))
            .With(x => x.Routes, GetExampleCollection<KongRoute>(CollectionExample.Valid))
            .With(x => x.Protocol, Protocol)
            .With(x => x.Host, Host)
            .With(x => x.Path, Path)
            .With(x => x.Retries, Retries)
            .With(x => x.ConnectTimeout, ConnectTimeout)
            .With(x => x.WriteTimeout, WriteTimeout)
            .With(x => x.ReadTimeout, ReadTimeout)
            .Create();

        protected IPostprocessComposer<KongService> BuildValidService() => Build<KongService>()
            .With(x => x.Protocol, "https")
            .With(x => x.Retries, 5)
            .With(x => x.ConnectTimeout, 1000U)
            .With(x => x.WriteTimeout, 1000U)
            .With(x => x.ReadTimeout, 1000U);
    }

    [Story(Title = nameof(KongService) + nameof(KongObject.ToJsonStringContent))]
    public class KongServiceSerializationScenarios : KongObjectSerializationScenarios<KongService>
    {
        [BddfyFact(DisplayName = nameof(ARandomInstance) + And + nameof(SerializingToStringContent))]
        public void Scenario1() =>
            this.Given(x => x.ARandomInstance())
                .When(x => x.SerializingToStringContent())
                .Then(x => x.RoutesIsNotSerialized())
                .And(x => x.PluginsIsNotSerialized())
                .And(x => x.PluginsIsNotNull())
                .And(x => x.RoutesIsNotNull())
                .BDDfy();

        protected void PluginsIsNotSerialized() => Serialized.Contains("\"plugins\":").Should().BeFalse();

        protected void RoutesIsNotSerialized() => Serialized.Contains("\"routes\":").Should().BeFalse();

        protected void PluginsIsNotNull() => Instance.Plugins.Should().NotBeNull();

        protected void RoutesIsNotNull() => Instance.Routes.Should().NotBeNull();
    }

    [Story(Title = nameof(KongService) + nameof(IKongvergeConfigObject.ToConfigJson))]
    public class KongServiceConfigSerializationScenarios : KongvergeConfigObjectSerializationScenarios<KongService> { }
}
