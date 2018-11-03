using Kongverge.DTOs;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;

namespace Kongverge.Tests.DTOs
{
    public abstract class KongPluginHostValidationScenarios<T> : ValidatableObjectSteps<T>
        where T : IKongPluginHost, IValidatableObject
    {
        protected CollectionExample Plugins;

        [BddfyFact(DisplayName = nameof(AValidInstanceWithExamplePlugins))]
        public void Scenario1() =>
            this.Given(x => x.AValidInstanceWithExamplePlugins())
                .When(x => x.Validating())
                .Then(x => x.TheErrorMessagesCountIsCorrect())
                .WithExamples(new ExampleTable(nameof(Plugins), nameof(ErrorMessagesCount))
                {
                    { CollectionExample.Null, 1 },
                    { CollectionExample.Empty, 0 },
                    { CollectionExample.Valid, 0 },
                    { CollectionExample.Invalid, 1 }
                })
                .BDDfy();

        protected abstract void AValidInstanceWithExamplePlugins();
    }

    public enum CollectionExample
    {
        Null,
        Empty,
        Valid,
        Invalid
    }
}
