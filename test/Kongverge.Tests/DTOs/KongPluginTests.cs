using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Kongverge.DTOs;
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

        protected void ConfigValuesAreShuffled()
        {
            OtherInstance.Config = new Dictionary<string, object>(OtherInstance.Config.Reverse());
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
        protected string PluginName;

        [BddfyFact(DisplayName = nameof(ARandomInstanceWithName))]
        public void Scenario1() =>
            this.Given(x => x.ARandomInstanceWithName(PluginName))
                .When(x => x.Validating())
                .Then(x => x.TheErrorMessagesCountIsCorrect())
                .WithExamples(new ExampleTable(nameof(PluginName), nameof(ErrorMessagesCount))
                {
                    { AvailablePlugins[0], 0 },
                    { Guid.NewGuid().ToString(), 1 }
                })
                .BDDfy();

        protected void ARandomInstanceWithName(string pluginName)
        {
            Instance = Build<KongPlugin>()
                .With(x => x.Name, pluginName)
                .Create();
        }
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
