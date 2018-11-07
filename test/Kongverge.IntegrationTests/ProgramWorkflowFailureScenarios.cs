using Kongverge.Helpers;
using Kongverge.Workflow;
using NCrunch.Framework;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace Kongverge.IntegrationTests
{
	[ExclusivelyUses(Host)]
    [Collection(Host)]
    [Story(Title = nameof(Program) + nameof(Program.Main) + nameof(KongvergeWorkflow) + "Failure")]
    public class ProgramWorkflowFailureScenarios : ProgramSteps
    {
        [BddfyFact(DisplayName = nameof(AValidHost) + And + nameof(AValidPort) + And + nameof(InputFolderIs) + InvalidDataA)]
        public void Scenario1() =>
            this.Given(x => x.AValidHost())
                .And(x => x.AValidPort())
                .And(x => x.InputFolderIs(InvalidDataA))
                .When(x => x.InvokingMain())
                .Then(x => x.TheExitCodeIs(ExitCode.InvalidConfigurationFiles))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(KongMatchesInputFolder) + A + And + nameof(AValidHost) + And + nameof(AValidPort) + And + nameof(InputFolderIs) + InvalidDataB)]
        public void Scenario2() =>
            this.Given(x => x.KongMatchesInputFolder(A))
                .And(x => x.AValidHost())
                .And(x => x.AValidPort())
                .And(x => x.InputFolderIs(InvalidDataB))
                .When(x => x.InvokingMain())
                .And(x => x.InvokingMainAgainForExport())
                .Then(x => x.TheExitCodeIs(ExitCode.Success))
                .And(x => x.OutputFolderContentsMatchesFolderContentsOf(A))
                .TearDownWith(s => KongIsBlank())
                .BDDfy();
    }
}
