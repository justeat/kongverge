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
        [BddfyFact(DisplayName = nameof(AValidHost) + And + nameof(TheRunCommand) + And + nameof(InputFolderIs) + InvalidData)]
        public void Scenario1() =>
            this.Given(x => x.AValidHost())
                .And(x => x.TheRunCommand())
                .And(x => x.InputFolderIs(InvalidData))
                .When(x => x.InvokingMain())
                .Then(x => x.TheExitCodeIs(ExitCode.InvalidConfigurationFiles))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(KongMatchesInputFolder) + A + And + nameof(AValidHost) + And + nameof(TheRunCommand) + And + nameof(InputFolderIs) + B + And + nameof(KongRespondsWithBadRequestAfterPartiallyApplyingNewConfiguration))]
        public void Scenario2() =>
            this.Given(x => x.KongMatchesInputFolder(A))
                .And(x => x.AValidHost())
                .And(x => x.TheRunCommand())
                .And(x => x.InputFolderIs(B))
                .And(x => x.KongRespondsWithBadRequestAfterPartiallyApplyingNewConfiguration())
                .When(x => x.InvokingMain())
                .And(x => x.TheExitCodeIs(ExitCode.UnspecifiedError))
                .And(x => x.InvokingMainAgainForExport())
                .Then(x => x.TheExitCodeIs(ExitCode.Success))
                .And(x => x.OutputFolderContentsMatchesFolderContentsOf(A))
                .TearDownWith(s => KongIsBlank())
                .BDDfy();
    }
}
