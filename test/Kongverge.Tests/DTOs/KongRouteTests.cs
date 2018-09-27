using System;
using System.Linq;
using System.Net.Http;
using AutoFixture;
using FluentAssertions;
using Kongverge.DTOs;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;

namespace Kongverge.Tests.DTOs
{
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

    public class KongRouteSerializationScenarios : SerializationScenarios<KongRoute>
    {
        [BddfyFact(DisplayName = nameof(ARandomInstance) + And + nameof(SerializingToStringContent))]
        public void Scenario2() =>
            this.Given(x => x.ARandomInstance())
                .When(x => x.SerializingToStringContent())
                .And(x => x.ServiceIsNotSerialized())
                .And(x => x.PluginsIsNotSerialized())
                .And(x => x.ServiceIsNotNull())
                .And(x => x.PluginsIsNotNull())
                .BDDfy();

        protected override StringContent MakeStringContent() => Instance.ToJsonStringContent();

        protected override string SerializingToConfigJson() => Serialized = Instance.ToConfigJson();

        protected void PluginsIsNotSerialized() => Serialized.Contains("\"plugins\":").Should().BeFalse();

        protected void ServiceIsNotNull() => Instance.Service.Should().NotBeNull();

        protected void PluginsIsNotNull() => Instance.Plugins.Should().NotBeNull();
    }
}
