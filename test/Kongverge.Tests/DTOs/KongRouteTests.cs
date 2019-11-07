using System;
using System.Linq;
using AutoFixture;
using AutoFixture.Dsl;
using FluentAssertions;
using Kongverge.DTOs;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;

namespace Kongverge.Tests.DTOs
{
    [Story(Title = nameof(KongRoute) + nameof(Equals))]
    public class KongRouteEqualityScenarios : EqualityScenarios<KongRoute>
    {
        protected override void ListValuesAreShuffled()
        {
            base.ListValuesAreShuffled();
            OtherInstance.Hosts = OtherInstance.Hosts.Reverse();
            OtherInstance.Protocols = OtherInstance.Protocols.Reverse();
            OtherInstance.Methods = OtherInstance.Methods.Reverse();
            OtherInstance.Paths = OtherInstance.Paths.Reverse();
            OtherInstance.Snis = OtherInstance.Snis.Reverse();
        }

        protected override void ListPropertiesAreEmptyAndNull()
        {
            base.ListPropertiesAreEmptyAndNull();
            Instance.Hosts = Array.Empty<string>();
            Instance.Protocols = Array.Empty<string>();
            Instance.Methods = Array.Empty<string>();
            Instance.Paths = Array.Empty<string>();
            Instance.Sources = Array.Empty<KongRoute.Endpoint>();
            Instance.Destinations = Array.Empty<KongRoute.Endpoint>();
            Instance.Snis = Array.Empty<string>();

            OtherInstance.Hosts = null;
            OtherInstance.Protocols = null;
            OtherInstance.Methods = null;
            OtherInstance.Paths = null;
            OtherInstance.Sources = null;
            OtherInstance.Destinations = null;
            OtherInstance.Snis = null;
        }

        protected override void OnlyThePersistenceValuesAreDifferent()
        {
            base.OnlyThePersistenceValuesAreDifferent();
            OtherInstance.Service = this.Create<KongObject.Reference>();
        }
    }

    [Story(Title = nameof(KongRoute) + nameof(IValidatableObject.Validate))]
    public class KongRouteValidationScenarios : ValidatableObjectSteps<KongRoute>
    {
        protected Children Plugins;
        protected Protocols Protocols;
        protected List Hosts;
        protected List Headers;
        protected Set Methods;
        protected List Paths;
        protected Set Snis;
        protected Set Sources;
        protected Set Destinations;
        protected ushort HttpsRedirect;

        [BddfyFact(DisplayName = nameof(AValidInstanceWithExamplePlugins))]
        public void Scenario1() =>
            this.Given(x => x.AValidInstanceWithExamplePlugins())
                .When(x => x.Validating())
                .Then(x => x.TheErrorMessagesCountIs(ErrorMessagesCount))
                .WithExamples(new ExampleTable(nameof(Plugins), nameof(ErrorMessagesCount))
                {
                    { Children.Null, 1 },
                    { Children.Empty, 0 },
                    { Children.Valid, 0 },
                    { Children.OneError, 1 }
                })
                .BDDfy();

        [BddfyFact(DisplayName = nameof(AnInstanceWithExamplePropertyValues))]
        public void Scenario2() =>
            this.Given(x => x.AnInstanceWithExamplePropertyValues())
                .When(x => x.Validating())
                .Then(x => x.TheErrorMessagesCountIs(ErrorMessagesCount))
                .WithExamples(new ExampleTable(
                nameof(Protocols), nameof(Hosts), nameof(Headers), nameof(Methods), nameof(Paths), nameof(Snis), nameof(Sources), nameof(Destinations), nameof(Tags), nameof(HttpsRedirect), nameof(ErrorMessagesCount))
                {
                    { Protocols.Null,         List.Null,         List.Null,         Set.Null,         List.Null,         Set.Null,         Set.Null,         Set.Null,         Set.Null,         999, 2  },
                    { Protocols.Empty,        List.Empty,        List.Empty,        Set.Empty,        List.Empty,        Set.Empty,        Set.Empty,        Set.Empty,        Set.Empty,        426, 1  },
                    { Protocols.Http,         List.Null,         List.Null,         Set.Null,         List.Null,         Set.ValidItems,   Set.ValidItems,   Set.ValidItems,   Set.ValidItems,   426, 1  },
                    { Protocols.Https,        List.Null,         List.Null,         Set.Null,         List.Null,         Set.Null,         Set.ValidItems,   Set.ValidItems,   Set.ValidItems,   301, 1  },
                    { Protocols.Tcp,          List.ValidItems,   List.ValidItems,   Set.ValidItems,   List.ValidItems,   Set.ValidItems,   Set.Null,         Set.Null,         Set.ValidItems,   302, 1  },
                    { Protocols.Tls,          List.ValidItems,   List.ValidItems,   Set.ValidItems,   List.ValidItems,   Set.Null,         Set.Null,         Set.Null,         Set.ValidItems,   307, 1  },
                    { Protocols.Grpc,         List.Null,         List.Null,         Set.ValidItems,   List.Null,         Set.ValidItems,   Set.ValidItems,   Set.ValidItems,   Set.ValidItems,   308, 1  },
                    { Protocols.Grpcs,        List.Null,         List.Null,         Set.ValidItems,   List.Null,         Set.Null,         Set.ValidItems,   Set.ValidItems,   Set.ValidItems,   426, 1  },
                    { Protocols.BothHttp,     List.Null,         List.Null,         Set.Null,         List.Null,         Set.ValidItems,   Set.Null,         Set.Null,         Set.ValidItems,   426, 1  },
                    { Protocols.BothTcp,      List.Null,         List.Null,         Set.Null,         List.Null,         Set.ValidItems,   Set.Null,         Set.Null,         Set.ValidItems,   426, 1  },
                    { Protocols.BothGrpc,     List.Null,         List.Null,         Set.Null,         List.Null,         Set.ValidItems,   Set.Null,         Set.Null,         Set.ValidItems,   426, 1  },
                    { Protocols.InvalidItems, List.ValidItems,   List.ValidItems,   Set.InvalidItems, List.ValidItems,   Set.InvalidItems, Set.InvalidItems, Set.InvalidItems, Set.InvalidItems, 426, 15 },
                    { Protocols.InvalidSet,   List.ValidItems,   List.ValidItems,   Set.InvalidSet,   List.ValidItems,   Set.InvalidSet,   Set.InvalidSet,   Set.InvalidSet,   Set.InvalidSet,   426, 6  }
                })
                .BDDfy();

        protected void AValidInstanceWithExamplePlugins() => Instance = BuildRouteWithoutPlugins(this)
            .With(x => x.Plugins, this.CreateChildren(Plugins, this.GetValidKongPlugin, this.GetKongPluginWithOneError))
            .Create();

        protected void AnInstanceWithExamplePropertyValues() => Instance = BuildRouteWithoutPlugins(this, Protocols, Hosts, Headers, Methods, Paths, Snis, Sources, Destinations, Tags, HttpsRedirect).Create();

        public static IPostprocessComposer<KongRoute> BuildRouteWithoutPlugins(
            Fixture fixture,
            Protocols protocols = Protocols.BothHttp,
            List hosts = List.ValidItems,
            List headers = List.Null,
            Set methods = Set.Null,
            List paths = List.Null,
            Set snis = Set.Null,
            Set sources = Set.Null,
            Set destinations = Set.Null,
            Set tags = Set.Null,
            ushort httpsRedirect = 426) => fixture.Build<KongRoute>()
            .With(x => x.Id, fixture.Create<string>())
            .With(x => x.HttpsRedirectStatusCode, httpsRedirect)
            .With(x => x.Plugins, fixture.CreateChildren(Children.Empty, fixture.GetValidKongPlugin, fixture.GetKongPluginWithOneError))
            .With(x => x.Protocols, fixture.CreateProtocols(protocols))
            .With(x => x.Hosts, fixture.CreateHosts(hosts))
            .With(x => x.Headers, fixture.CreateHeaders(headers))
            .With(x => x.Methods, fixture.CreateMethods(methods))
            .With(x => x.Paths, fixture.CreatePaths(paths))
            .With(x => x.Snis, fixture.CreateSnis(snis))
            .With(x => x.Sources, fixture.CreateEndpoints(sources))
            .With(x => x.Destinations, fixture.CreateEndpoints(destinations))
            .With(x => x.Tags, fixture.CreateTags(tags));
    }

    [Story(Title = nameof(KongRoute) + nameof(KongObject.ToJsonStringContent))]
    public class KongRouteSerializationScenarios : KongObjectSerializationScenarios<KongRoute>
    {
        [BddfyFact(DisplayName = nameof(ARandomInstance) + And + nameof(SerializingToStringContent))]
        public void Scenario1() =>
            this.Given(x => x.ARandomInstance())
                .When(x => x.SerializingToStringContent())
                .Then(x => x.ServiceReferenceIsSerialized())
                .And(x => x.PluginsIsNotSerialized())
                .And(x => x.PluginsIsNotNull())
                .BDDfy();

        protected void ServiceReferenceIsSerialized() => Serialized.Should().Contain("\"service\":");

        protected void PluginsIsNotSerialized() => Serialized.Should().NotContain("\"plugins\":");

        protected void PluginsIsNotNull() => Instance.Plugins.Should().NotBeNull();
    }
}
