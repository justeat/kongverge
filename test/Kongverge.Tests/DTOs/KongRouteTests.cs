using System;
using System.Linq;
using AutoFixture;
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
                .Then(x => x.TheErrorMessagesCountIsCorrect())
                .WithExamples(new ExampleTable(nameof(Protocols), nameof(Hosts), nameof(Methods), nameof(Paths), nameof(ErrorMessagesCount))
                {
                    { ProtocolsExample.Null, StringsExample.Null, StringsExample.Null, StringsExample.Null, 5 },
                    { ProtocolsExample.Empty, StringsExample.Empty, StringsExample.Empty, StringsExample.Empty, 2 },
                    { ProtocolsExample.Invalid, StringsExample.Valid, StringsExample.Valid, StringsExample.Valid, 1 },
                    { ProtocolsExample.Http, StringsExample.Valid, StringsExample.Valid, StringsExample.Valid, 0 },
                    { ProtocolsExample.Https, StringsExample.Valid, StringsExample.Valid, StringsExample.Valid, 0 },
                    { ProtocolsExample.Both, StringsExample.Valid, StringsExample.Empty, StringsExample.Empty, 0 },
                    { ProtocolsExample.Both, StringsExample.Empty, StringsExample.Valid, StringsExample.Empty, 0 },
                    { ProtocolsExample.Both, StringsExample.Empty, StringsExample.Empty, StringsExample.Valid, 0 },
                    { ProtocolsExample.Invalid, StringsExample.Invalid, StringsExample.Valid, StringsExample.Valid, 2 },
                    { ProtocolsExample.Invalid, StringsExample.Valid, StringsExample.Invalid, StringsExample.Valid, 2 },
                    { ProtocolsExample.Invalid, StringsExample.Valid, StringsExample.Valid, StringsExample.Invalid, 2 },
                    { ProtocolsExample.Invalid, StringsExample.Invalid, StringsExample.Empty, StringsExample.Invalid, 3 },
                    { ProtocolsExample.Invalid, StringsExample.Invalid, StringsExample.Invalid, StringsExample.Invalid, 4 }
                })
                .BDDfy();

        protected override void AValidInstanceWithExamplePlugins() => Instance = Build<KongRoute>()
            .With(x => x.Plugins, GetExampleCollection<KongPlugin>(Plugins))
            .With(x => x.Protocols, GetProtocolsExample(ProtocolsExample.Both))
            .With(x => x.Hosts, GetStringsExample(StringsExample.Empty))
            .With(x => x.Methods, GetStringsExample(StringsExample.Empty))
            .With(x => x.Paths, GetStringsExample(StringsExample.Valid))
            .Create();

        protected void AnInstanceWithExamplePropertyValues()
        {
            Instance = Build<KongRoute>()
                .With(x => x.Plugins, GetExampleCollection<KongPlugin>(CollectionExample.Empty))
                .With(x => x.Protocols, GetProtocolsExample(Protocols))
                .With(x => x.Hosts, GetStringsExample(Hosts))
                .With(x => x.Methods, GetStringsExample(Methods))
                .With(x => x.Paths, GetStringsExample(Paths))
                .Create();
        }

        protected string[] GetProtocolsExample(ProtocolsExample example)
        {
            switch (example)
            {
                case ProtocolsExample.Null:
                    return null;
                case ProtocolsExample.Empty:
                    return Array.Empty<string>();
                case ProtocolsExample.Http:
                    return new[] { "http" };
                case ProtocolsExample.Https:
                    return new[] { "https" };
                case ProtocolsExample.Both:
                    return new[] { "http", "https" };
                case ProtocolsExample.Invalid:
                    return new[] { "junk" };
                default:
                    throw new ArgumentOutOfRangeException(nameof(example), example, null);
            }
        }

        protected string[] GetStringsExample(StringsExample example)
        {
            switch (example)
            {
                case StringsExample.Null:
                    return null;
                case StringsExample.Empty:
                    return Array.Empty<string>();
                case StringsExample.Valid:
                    return new[] { "valid1", "valid2", "valid3" };
                case StringsExample.Invalid:
                    return new[] { null, string.Empty, " ", ":" };
                default:
                    throw new ArgumentOutOfRangeException(nameof(example), example, null);
            }
        }

        public enum ProtocolsExample
        {
            Null,
            Empty,
            Http,
            Https,
            Both,
            Invalid
        }

        public enum StringsExample
        {
            Null,
            Empty,
            Valid,
            Invalid
        }
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
