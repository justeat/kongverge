using AutoFixture;
using Kongverge.DTOs;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;

namespace Kongverge.Tests.DTOs
{
    [Story(Title = nameof(GlobalConfig) + nameof(IValidatableObject.Validate))]
    public class GlobalConfigValidationScenarios : ValidatableObjectSteps<GlobalConfig>
    {
        protected Children Plugins;
        protected Children Consumers;

        [BddfyFact(DisplayName = nameof(AValidInstanceWithExamplePluginsAndConsumers))]
        public void Scenario1() =>
            this.Given(x => x.AValidInstanceWithExamplePluginsAndConsumers())
                .When(x => x.Validating())
                .Then(x => x.TheErrorMessagesCountIs(ErrorMessagesCount))
                .WithExamples(new ExampleTable(nameof(Plugins), nameof(Consumers), nameof(ErrorMessagesCount))
                {
                    { Children.Null,     Children.Null,     2 },
                    { Children.Empty,    Children.Empty,    0 },
                    { Children.Valid,    Children.Valid,    0 },
                    { Children.OneError, Children.Valid,    1 },
                    { Children.Valid,    Children.OneError, 1 },
                    { Children.OneError, Children.OneError, 2 }
                })
                .BDDfy();

        protected void AValidInstanceWithExamplePluginsAndConsumers() => Instance = Build<GlobalConfig>()
            .With(x => x.Plugins, this.CreateChildren(Plugins, this.GetValidKongPlugin, this.GetKongPluginWithOneError))
            .With(x => x.Consumers, this.CreateChildren(Consumers, this.GetValidKongConsumer, this.GetKongConsumerWithOneError))
            .Create();

    }

    [Story(Title = nameof(GlobalConfig) + nameof(IKongvergeConfigObject.ToConfigJson))]
    public class GlobalConfigSerializationScenarios : KongvergeConfigObjectSerializationScenarios<GlobalConfig> { }
}
