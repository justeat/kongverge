using AutoFixture;
using Kongverge.DTOs;
using Moq;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;

namespace Kongverge.Tests.DTOs
{
    public abstract class KongPluginHostValidationScenarios<T> : ValidatableObjectSteps<T>
        where T : IKongPluginHost, IValidatableObject
    {
        protected Mock<KongPlugin> PluginMock = new Mock<KongPlugin>();

        protected bool PluginsValid;

        [BddfyFact(DisplayName = nameof(ARandomInstanceWithMockedPlugins))]
        public void Scenario1() =>
            this.Given(x => x.ARandomInstanceWithMockedPlugins())
                .When(x => x.Validating())
                .Then(x => x.TheErrorMessagesCountIsCorrect())
                .WithExamples(new ExampleTable(nameof(PluginsValid), nameof(ErrorMessagesCount))
                {
                    { true, 0 },
                    { false, 1 }
                })
                .BDDfy();

        protected void ARandomInstanceWithMockedPlugins()
        {
            SetupMock(PluginMock, PluginsValid);

            Instance = Build<T>()
                .With(x => x.Plugins, new[] { PluginMock.Object })
                .Create();
        }
    }
}
