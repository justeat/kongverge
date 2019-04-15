using System;
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
        [BddfyFact(DisplayName = nameof(ARandomInstance) + And + nameof(AnotherInstanceClonedFromTheFirst) + And + nameof(ConfigValuesAreShuffled))]
        public void Scenario4() =>
            this.Given(x => x.ARandomInstance())
                .And(x => x.AnotherInstanceClonedFromTheFirst())
                .And(x => x.ConfigValuesAreShuffled())
                .When(x => x.CheckingEquality())
                .And(x => x.CheckingHashCodes())
                .Then(x => x.TheyAreEqual())
                .BDDfy();

        protected override void ARandomInstance() => Instance = Build<KongPlugin>()
            .With(x => x.Config, JObject.FromObject(this.Create<Dictionary<string, string>>()))
            .Create();

        protected void ConfigValuesAreShuffled()
        {
            var otherConfig = OtherInstance.Config.ToObject<Dictionary<string, object>>();
            OtherInstance.Config = JObject.FromObject(new Dictionary<string, object>(otherConfig.Reverse()));
        }

        protected override void OnlyThePersistenceValuesAreDifferent()
        {
            OtherInstance.Id = this.Create<string>();
            OtherInstance.CreatedAt = this.Create<long>();
            OtherInstance.ConsumerId = this.Create<string>();
            OtherInstance.ServiceId = this.Create<string>();
            OtherInstance.RouteId = this.Create<string>();
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

        [BddfyFact(DisplayName = nameof(AValidInstanceWithMissingDefaultConfigFields))]
        public void Scenario3() =>
            this.Given(x => x.AValidInstanceWithMissingDefaultConfigFields())
                .When(x => x.Validating())
                .Then(x => x.TheErrorMessagesCountIs(0))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(AValidInstanceWithTwoInvalidConfigFields))]
        public void Scenario4() =>
            this.Given(x => x.AValidInstanceWithTwoInvalidConfigFields())
                .When(x => x.Validating())
                .Then(x => x.TheErrorMessagesCountIs(2))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(AnInstanceWithThreeUnknownConfigFields))]
        public void Scenario5() =>
            this.Given(x => x.AnInstanceWithThreeUnknownConfigFields())
                .When(x => x.Validating())
                .Then(x => x.TheErrorMessagesCountIs(3))
                .BDDfy();

        protected void AValidInstance() => Instance = new KongPlugin
        {
            Name = "example",
            Config = JObject.FromObject(new
            {
                field1 = 1,
                field2 = this.Create<string>(),
                field3 = new
                {
                    field1 = this.Create<bool>(),
                    field2 = this.Create<string>()
                },
                field4 = this.Create<string[]>()
            })
        };

        protected void AnUnavailableInstance() => Instance = new KongPlugin
        {
            Name = "nonexistent",
            Config = new JObject()
        };

        protected void AValidInstanceWithMissingDefaultConfigFields()
        {
            Instance = new KongPlugin
            {
                Name = "example",
                Config = JObject.FromObject(new
                {
                    field2 = this.Create<string>(),
                    field3 = new
                    {
                        field1 = this.Create<bool>()
                    },
                    field4 = this.Create<object>() // A blank object is valid for an array
                })
            };
        }

        protected void AValidInstanceWithTwoInvalidConfigFields() => Instance = new KongPlugin
        {
            Name = "example",
            Config = JObject.FromObject(new
            {
                field1 = 1,
                field2 = this.Create<string>(),
                field3 = this.Create<string>(),
                field4 = this.Create<string>()
            })
        };

        protected void AnInstanceWithThreeUnknownConfigFields() => Instance = new KongPlugin
        {
            Name = "example",
            Config = JObject.FromObject(new
            {
                field1 = this.Create<int>(),
                field2 = this.Create<string>(),
                field3 = new
                {
                    field1 = this.Create<bool>(),
                    field2 = this.Create<string>(),
                    field3 = this.Create<bool>()
                },
                field4 = this.Create<string[]>(),
                field5 = this.Create<bool>(),
                field6 = this.Create<bool>()
            })
        };
    }

    [Story(Title = nameof(KongPlugin) + nameof(KongObject.ToJsonStringContent))]
    public class KongPluginSerializationScenarios : KongObjectSerializationScenarios<KongPlugin>
    {
        [BddfyFact(DisplayName = nameof(ARandomInstance) + And + nameof(SerializingToStringContent))]
        public void Scenario1() =>
            this.Given(x => x.ARandomInstance())
                .When(x => x.SerializingToStringContent())
                .Then(x => x.ItSerializesWithoutError())
                .BDDfy();

        protected void ItSerializesWithoutError() => Serialized.Should().NotBeEmpty();
    }
}
