using Kongverge.DTOs;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;

namespace Kongverge.Tests.DTOs
{
    public abstract class KongPluginHostValidationScenarios<T> : ValidatableObjectSteps<T>
        where T : IKongPluginHost, IValidatableObject
    {
        protected Children Plugins;

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

        protected abstract void AValidInstanceWithExamplePlugins();
    }
}
