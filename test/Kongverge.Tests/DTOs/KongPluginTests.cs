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

        [BddfyFact(DisplayName = nameof(AValidInstanceWithOneInvalidConfigField))]
        public void Scenario4() =>
            this.Given(x => x.AValidInstanceWithOneInvalidConfigField())
                .When(x => x.Validating())
                .Then(x => x.TheErrorMessagesCountIs(1))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(AnInstanceWithTwoUnknownConfigFields))]
        public void Scenario5() =>
            this.Given(x => x.AnInstanceWithTwoUnknownConfigFields())
                .When(x => x.Validating())
                .Then(x => x.TheErrorMessagesCountIs(2))
                .BDDfy();

        protected void AValidInstance() => Instance = ExamplePlugin;

        protected void AnUnavailableInstance()
        {
            Instance = ExamplePlugin;
            Instance.Name = Guid.NewGuid().ToString();
        }

        protected void AValidInstanceWithMissingDefaultConfigFields() => Instance = ExamplePluginWithMissingDefaultConfigFields;

        protected void AValidInstanceWithOneInvalidConfigField() => Instance = ExamplePluginWithOneInvalidConfigField;

        protected void AnInstanceWithTwoUnknownConfigFields() => Instance = ExamplePluginWithTwoUnknownConfigFields;
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
