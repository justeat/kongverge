using Kongverge.Helpers;
using Kongverge.Workflow;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace Kongverge.IntegrationTests
{
    [Collection(Host)]
    [Story(Title = nameof(Program) + nameof(Program.Main) + nameof(KongvergeWorkflow) + "Failure")]
    public class ProgramWorkflowFailureScenarios : ProgramSteps
    {
        [BddfyFact(DisplayName = nameof(AValidHost) + And + nameof(AValidPort) + And + nameof(InputFolderIs) + InvalidData)]
        public void Scenario1() =>
            this.Given(x => x.AValidHost())
                .And(x => x.AValidPort())
                .And(x => x.InputFolderIs(InvalidData))
                .When(x => x.InvokingMain())
                .Then(x => x.TheExitCodeIs(ExitCode.InvalidConfigurationFiles))
                .BDDfy();
    }
}
