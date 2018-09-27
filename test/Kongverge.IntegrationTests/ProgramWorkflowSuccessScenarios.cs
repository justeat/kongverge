using Kongverge.Helpers;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace Kongverge.IntegrationTests
{
    [Collection(Host)]
    public class ProgramWorkflowSuccessScenarios : ProgramSteps
    {
        [BddfyFact(DisplayName = nameof(KongIsBlank) + And + nameof(AValidHost) + And + nameof(AValidPort) + And  + nameof(InputFolderIs) + A)]
        public void Scenario1() =>
            this.Given(x => x.KongIsBlank())
                .And(x => x.AValidHost())
                .And(x => x.AValidPort())
                .And(x => x.InputFolderIs(A))
                .When(x => x.InvokingMain())
                .And(x => x.InvokingMainAgainForExport())
                .Then(x => x.TheExitCodeIs(ExitCode.Success))
                .And(x => x.OutputFolderContentsMatchInputFolderContents())
                .TearDownWith(s => KongIsBlank())
                .BDDfy();

        [BddfyFact(DisplayName = nameof(KongIsBlank) + And + nameof(AValidHost) + And + nameof(AValidPort) + And  + nameof(InputFolderIs) + B)]
        public void Scenario2() =>
            this.Given(x => x.KongIsBlank())
                .And(x => x.AValidHost())
                .And(x => x.AValidPort())
                .And(x => x.InputFolderIs(B))
                .When(x => x.InvokingMain())
                .And(x => x.InvokingMainAgainForExport())
                .Then(x => x.TheExitCodeIs(ExitCode.Success))
                .And(x => x.OutputFolderContentsMatchInputFolderContents())
                .TearDownWith(s => KongIsBlank())
                .BDDfy();

        [BddfyFact(DisplayName = nameof(KongMatchesInputFolder) + A + And + nameof(AValidHost) + And + nameof(AValidPort) + And + nameof(InputFolderIs) + B)]
        public void Scenario3() =>
            this.Given(x => x.KongMatchesInputFolder(A))
                .And(x => x.AValidHost())
                .And(x => x.AValidPort())
                .And(x => x.InputFolderIs(B))
                .When(x => x.InvokingMain())
                .And(x => x.InvokingMainAgainForExport())
                .Then(x => x.TheExitCodeIs(ExitCode.Success))
                .And(x => x.OutputFolderContentsMatchInputFolderContents())
                .TearDownWith(s => KongIsBlank())
                .BDDfy();

        [BddfyFact(DisplayName = nameof(KongMatchesInputFolder) + B + And + nameof(AValidHost) + And + nameof(AValidPort) + And + nameof(InputFolderIs) + A)]
        public void Scenario4() =>
            this.Given(x => x.KongMatchesInputFolder(B))
                .And(x => x.AValidHost())
                .And(x => x.AValidPort())
                .And(x => x.InputFolderIs(A))
                .When(x => x.InvokingMain())
                .And(x => x.InvokingMainAgainForExport())
                .Then(x => x.TheExitCodeIs(ExitCode.Success))
                .And(x => x.OutputFolderContentsMatchInputFolderContents())
                .TearDownWith(s => KongIsBlank())
                .BDDfy();
    }
}
