using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Kongverge.DTOs;
using Newtonsoft.Json.Linq;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;

namespace Kongverge.Tests.DTOs
{
    [Story(Title = nameof(KongPlugin) + nameof(Equals))]
    public class KongPluginEqualityScenarios : EqualityScenarios<KongPlugin>
    {
        protected override void ARandomInstance() => Instance = Build<KongPlugin>()
            .With(x => x.Config, JObject.FromObject(this.Create<Dictionary<string, string>>()))
            .Create();

        protected override void ListValuesAreShuffled()
        {
            base.ListValuesAreShuffled();
            var otherConfig = OtherInstance.Config.ToObject<Dictionary<string, object>>();
            OtherInstance.Config = JObject.FromObject(new Dictionary<string, object>(otherConfig.Reverse()));
        }

        protected override void OnlyThePersistenceValuesAreDifferent()
        {
            base.OnlyThePersistenceValuesAreDifferent();
            OtherInstance.Consumer = new KongObject.Reference { Id = this.Create<string>() };
            OtherInstance.Service = new KongObject.Reference { Id = this.Create<string>() };
            OtherInstance.Route = new KongObject.Reference { Id = this.Create<string>() };
        }
    }

    [Story(Title = nameof(KongPlugin) + nameof(IValidatableObject.Validate))]
    public class KongPluginValidationScenarios : ValidatableObjectSteps<KongPlugin>
    {
        [BddfyFact(DisplayName = nameof(AValidInstance))]
        public void Scenario1() =>
            this.Given(x => x.AValidInstance())
                .When(x => x.Validating())
                .Then(x => x.TheErrorMessagesCountIs(0))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(AnUnavailableInstance))]
        public void Scenario2() =>
            this.Given(x => x.AnUnavailableInstance())
                .When(x => x.Validating())
                .Then(x => x.TheErrorMessagesCountIs(1))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(AValidInstanceWithMissingDefaultFields))]
        public void Scenario3() =>
            this.Given(x => x.AValidInstanceWithMissingDefaultFields())
                .When(x => x.Validating())
                .Then(x => x.TheMissingDefaultFieldsAreSubstitutedWithTheirDefaults())
                .Then(x => x.TheErrorMessagesCountIs(0))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(AValidInstanceWithTwelveInvalidFields))]
        public void Scenario4() =>
            this.Given(x => x.AValidInstanceWithTwelveInvalidFields())
                .When(x => x.Validating())
                .Then(x => x.TheErrorMessagesCountIs(12))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(AnInstanceWithThreeUnknownFields))]
        public void Scenario5() =>
            this.Given(x => x.AnInstanceWithThreeUnknownFields())
                .When(x => x.Validating())
                .Then(x => x.TheErrorMessagesCountIs(3))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(AnInstanceWithInvalidParent))]
        public void Scenario6() =>
            this.Given(x => x.AnInstanceWithInvalidParent())
                .When(x => x.Validating())
                .Then(x => x.TheErrorMessagesCountIs(1))
                .BDDfy();

        protected void AValidInstance() => Instance = this.GetValidKongPlugin();

        protected void AnUnavailableInstance() => Instance = new KongPlugin
        {
            Name = "nonexistent",
            Config = new JObject()
        };
        
        protected void AValidInstanceWithMissingDefaultFields()
        {
            AValidInstance();
            Instance.Config.Property("field1").Remove();
        }

        protected void TheMissingDefaultFieldsAreSubstitutedWithTheirDefaults()
        {
            Instance.RunOn.Should().Be("first");
            Instance.Protocols.Should().BeEquivalentTo("grpc", "grpcs", "http", "https");
        }

        protected void AValidInstanceWithTwelveInvalidFields()
        {
            AValidInstance();
            Instance.RunOn = this.Create<string>();
            Instance.Protocols = new[] { this.Create<string>() };
            Instance.Config["field3"] = JToken.FromObject(this.Create<string>());
            Instance.Config["field4"] = JToken.FromObject(new[] { "foo" });
            Instance.Config["field5"] = JToken.FromObject(this.Create<int>() + 10);
            Instance.Config["field7"] = JToken.FromObject(this.Create<string>());
            ((JObject)Instance.Config["field8"]).Add("something", JToken.FromObject(new
            {
                field1 = this.Create<string>().Substring(0, 5),
                field2 = this.Create<string>().Substring(0, 5)
            }));
            ((JObject)Instance.Config["field8"]).Add("another", JToken.FromObject(new object()));
            Instance.Config["field9"] = JToken.FromObject(5);
            Instance.Config["field10"] = JToken.FromObject(new object());
            Instance.Config.Children().Last().Remove();
        }

        protected void AnInstanceWithThreeUnknownFields()
        {
            AValidInstance();
            ((JObject)Instance.Config["field3"]).Add("field3", JToken.FromObject(this.Create<bool>()));
            Instance.Config.Add("field12", JToken.FromObject(this.Create<bool>()));
            Instance.Config.Add("field13", JToken.FromObject(this.Create<bool>()));
        }

        protected void AnInstanceWithInvalidParent()
        {
            AValidInstance();
            Parent = new KongConsumer();
        }
    }

    [Story(Title = nameof(KongPlugin) + nameof(KongObject.ToJsonStringContent))]
    public class KongPluginSerializationScenarios : KongObjectSerializationScenarios<KongPlugin>
    {
        [BddfyFact(DisplayName = nameof(ARandomInstance) + And + nameof(SerializingToStringContent))]
        public void Scenario1() =>
            this.Given(x => x.ARandomInstance())
                .When(x => x.SerializingToStringContent())
                .Then(x => x.ServiceReferenceIsSerialized())
                .And(x => x.RouteReferenceIsSerialized())
                .And(x => x.ConsumerReferenceIsSerialized())
                .BDDfy();

        protected void ServiceReferenceIsSerialized() => Serialized.Should().Contain("\"service\":");

        protected void RouteReferenceIsSerialized() => Serialized.Should().Contain("\"route\":");

        protected void ConsumerReferenceIsSerialized() => Serialized.Should().Contain("\"consumer\":");
    }
}
