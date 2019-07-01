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
        [BddfyFact(DisplayName = nameof(ARandomInstance) + And + nameof(AnotherInstanceClonedFromTheFirst) + And + nameof(ListValuesAreShuffled))]
        public void Scenario4() =>
            this.Given(x => x.ARandomInstance())
                .And(x => x.AnotherInstanceClonedFromTheFirst())
                .And(x => x.ListValuesAreShuffled())
                .When(x => x.CheckingEquality())
                .And(x => x.CheckingHashCodes())
                .Then(x => x.TheyAreEqual())
                .BDDfy();

        [BddfyFact(DisplayName = nameof(ARandomInstance) + And + nameof(AnotherInstanceClonedFromTheFirst) + And + nameof(ListPropertiesAreEmptyAndNull))]
        public void Scenario5() =>
            this.Given(x => x.ARandomInstance())
                .And(x => x.AnotherInstanceClonedFromTheFirst())
                .And(x => x.ListPropertiesAreEmptyAndNull())
                .When(x => x.CheckingEquality())
                .And(x => x.CheckingHashCodes())
                .Then(x => x.TheyAreEqual())
                .BDDfy();

        protected void ListValuesAreShuffled()
        {
            OtherInstance.Hosts = OtherInstance.Hosts.Reverse();
            OtherInstance.Protocols = OtherInstance.Protocols.Reverse();
            OtherInstance.Methods = OtherInstance.Methods.Reverse();
            OtherInstance.Paths = OtherInstance.Paths.Reverse();
        }

        protected void ListPropertiesAreEmptyAndNull()
        {
            Instance.Hosts = Array.Empty<string>();
            Instance.Protocols = Array.Empty<string>();
            Instance.Methods = Array.Empty<string>();
            Instance.Paths = Array.Empty<string>();

            OtherInstance.Hosts = null;
            OtherInstance.Protocols = null;
            OtherInstance.Methods = null;
            OtherInstance.Paths = null;
        }

        protected override void OnlyThePersistenceValuesAreDifferent()
        {
            OtherInstance.Id = this.Create<string>();
            OtherInstance.Service = this.Create<KongRoute.ServiceReference>();
            OtherInstance.CreatedAt = this.Create<long>();
        }
    }

    [Story(Title = nameof(KongRoute) + nameof(IValidatableObject.Validate))]
    public class KongRouteValidationScenarios : KongPluginHostValidationScenarios<KongRoute>
    {
        protected ProtocolsExample Protocols;
        protected StringsExample Hosts;
        protected StringsExample Methods;
        protected StringsExample Paths;

        [BddfyFact(DisplayName = nameof(AnInstanceWithExamplePropertyValues))]
        public void Scenario2() =>
            this.Given(x => x.AnInstanceWithExamplePropertyValues())
                .When(x => x.Validating())
                .Then(x => x.TheErrorMessagesCountIs(ErrorMessagesCount))
                .WithExamples(new ExampleTable(nameof(Protocols), nameof(Hosts), nameof(Methods), nameof(Paths), nameof(ErrorMessagesCount))
                {
                    { ProtocolsExample.Null, StringsExample.InvalidAll, StringsExample.InvalidAll, StringsExample.InvalidAll, 7 },
                    { ProtocolsExample.Empty, StringsExample.Empty, StringsExample.Empty, StringsExample.Empty, 2 },
                    { ProtocolsExample.Invalid, StringsExample.ValidHosts, StringsExample.ValidMethods, StringsExample.ValidPaths, 1 },
                    { ProtocolsExample.Http, StringsExample.ValidHosts, StringsExample.ValidMethods, StringsExample.ValidPaths, 0 },
                    { ProtocolsExample.Https, StringsExample.ValidHosts, StringsExample.ValidMethods, StringsExample.ValidPaths, 0 },
                    { ProtocolsExample.Both, StringsExample.ValidHosts, StringsExample.Empty, StringsExample.Empty, 0 },
                    { ProtocolsExample.Both, StringsExample.Empty, StringsExample.ValidMethods, StringsExample.Empty, 0 },
                    { ProtocolsExample.Both, StringsExample.Empty, StringsExample.Empty, StringsExample.ValidPaths, 0 },
                    { ProtocolsExample.Invalid, StringsExample.InvalidNull, StringsExample.ValidMethods, StringsExample.ValidPaths, 2 },
                    { ProtocolsExample.Invalid, StringsExample.InvalidColon, StringsExample.Empty, StringsExample.Empty, 2 },
                    { ProtocolsExample.Invalid, StringsExample.InvalidWildcard, StringsExample.Empty, StringsExample.Empty, 2 },
                    { ProtocolsExample.Invalid, StringsExample.ValidHosts, StringsExample.InvalidNull, StringsExample.ValidPaths, 2 },
                    { ProtocolsExample.Invalid, StringsExample.ValidHosts, StringsExample.ValidMethods, StringsExample.InvalidNull, 2 },
                    { ProtocolsExample.Invalid, StringsExample.InvalidEmpty, StringsExample.Empty, StringsExample.InvalidEmpty, 3 },
                    { ProtocolsExample.Invalid, StringsExample.LeadingAndTrailingWildcards, StringsExample.InvalidEmpty, StringsExample.InvalidWhitespace, 4 },
                    { ProtocolsExample.Invalid, StringsExample.InvalidWhitespace, StringsExample.InvalidWhitespace, StringsExample.InvalidPaths, 4 }
                })
                .BDDfy();

        protected override void AValidInstanceWithExamplePlugins() => Instance = BuildValidRouteWithoutPlugins(this)
            .With(x => x.Plugins, this.GetExampleCollection(Plugins, this.GetValidKongPlugin, this.GetKongPluginWithOneError))
            .Create();

        protected void AnInstanceWithExamplePropertyValues() => Instance = Build<KongRoute>()
            .With(x => x.Plugins, this.GetExampleCollection(CollectionExample.Empty, this.GetValidKongPlugin, this.GetKongPluginWithOneError))
            .With(x => x.Protocols, this.GetExampleProtocols(Protocols))
            .With(x => x.Hosts, this.GetExampleStrings(Hosts))
            .With(x => x.Methods, this.GetExampleStrings(Methods))
            .With(x => x.Paths, this.GetExampleStrings(Paths))
            .Create();

        public static IPostprocessComposer<KongRoute> BuildValidRouteWithoutPlugins(Fixture fixture) => fixture.Build<KongRoute>()
            .With(x => x.Plugins, fixture.GetExampleCollection(CollectionExample.Empty, fixture.GetValidKongPlugin, fixture.GetKongPluginWithOneError))
            .With(x => x.Protocols, fixture.GetExampleProtocols(ProtocolsExample.Both))
            .With(x => x.Hosts, fixture.GetExampleStrings(StringsExample.Empty))
            .With(x => x.Methods, fixture.GetExampleStrings(StringsExample.Empty))
            .With(x => x.Paths, fixture.GetExampleStrings(StringsExample.ValidPaths));
    }

    [Story(Title = nameof(KongRoute) + nameof(KongObject.ToJsonStringContent))]
    public class KongRouteSerializationScenarios : KongObjectSerializationScenarios<KongRoute>
    {
        [BddfyFact(DisplayName = nameof(ARandomInstance) + And + nameof(SerializingToStringContent))]
        public void Scenario1() =>
            this.Given(x => x.ARandomInstance())
                .When(x => x.SerializingToStringContent())
                .Then(x => x.ServiceIsNotSerialized())
                .And(x => x.PluginsIsNotSerialized())
                .And(x => x.ServiceIsNotNull())
                .And(x => x.PluginsIsNotNull())
                .BDDfy();

        protected void PluginsIsNotSerialized() => Serialized.Contains("\"plugins\":").Should().BeFalse();

        protected void ServiceIsNotNull() => Instance.Service.Should().NotBeNull();

        protected void PluginsIsNotNull() => Instance.Plugins.Should().NotBeNull();
    }
}
