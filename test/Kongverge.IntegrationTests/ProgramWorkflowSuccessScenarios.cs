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
    [Story(Title = nameof(Program) + nameof(Program.Main) + nameof(KongvergeWorkflow) + "Success")]
    public class ProgramWorkflowSuccessScenarios : ProgramSteps
    {
        [BddfyFact(DisplayName = nameof(KongIsBlank) + And + nameof(AValidHost) + And + nameof(TheRunCommand) + And + nameof(InputFolderIs) + A)]
        public void Scenario1() =>
            this.Given(x => x.KongIsBlank())
                .And(x => x.AValidHost())
                .And(x => x.TheRunCommand())
                .And(x => x.InputFolderIs(A))
                .When(x => x.InvokingMain())
                .And(x => x.TheExitCodeIs(ExitCode.Success))
                .And(x => x.InvokingMainAgainForExport())
                .Then(x => x.TheExitCodeIs(ExitCode.Success))
                .And(x => x.OutputFolderContentsMatchInputFolderContents())
                .TearDownWith(s => KongIsBlank())
                .BDDfy();

        [BddfyFact(DisplayName = nameof(KongIsBlank) + And + nameof(AValidHost) + And + nameof(TheRunCommand) + And + nameof(InputFolderIs) + B)]
        public void Scenario2() =>
            this.Given(x => x.KongIsBlank())
                .And(x => x.AValidHost())
                .And(x => x.TheRunCommand())
                .And(x => x.InputFolderIs(B))
                .When(x => x.InvokingMain())
                .And(x => x.TheExitCodeIs(ExitCode.Success))
                .And(x => x.InvokingMainAgainForExport())
                .Then(x => x.TheExitCodeIs(ExitCode.Success))
                .And(x => x.OutputFolderContentsMatchInputFolderContents())
                .TearDownWith(s => KongIsBlank())
                .BDDfy();

        [BddfyFact(DisplayName = nameof(KongIsBlank) + And + nameof(AValidHost) + And + nameof(TheRunCommand) + And + nameof(FaultToleranceOn) + And + nameof(InputFolderIs) + C)]
        public void Scenario3() =>
            this.Given(x => x.KongIsBlank())
                .And(x => x.AValidHost())
                .And(x => x.TheRunCommand())
                .And(x => x.InputFolderIs(C))
                .When(x => x.InvokingMain())
                .And(x => x.TheExitCodeIs(ExitCode.Success))
                .TearDownWith(s => KongIsBlank())
                .BDDfy();

        [BddfyFact(DisplayName = nameof(KongMatchesInputFolder) + A + And + nameof(AValidHost) + And + nameof(TheRunCommand) + And + nameof(InputFolderIs) + B)]
        public void Scenario4() =>
            this.Given(x => x.KongMatchesInputFolder(A))
                .And(x => x.AValidHost())
                .And(x => x.TheRunCommand())
                .And(x => x.InputFolderIs(B))
                .When(x => x.InvokingMain())
                .And(x => x.TheExitCodeIs(ExitCode.Success))
                .And(x => x.InvokingMainAgainForExport())
                .Then(x => x.TheExitCodeIs(ExitCode.Success))
                .And(x => x.OutputFolderContentsMatchInputFolderContents())
                .TearDownWith(s => KongIsBlank())
                .BDDfy();

        [BddfyFact(DisplayName = nameof(KongMatchesInputFolder) + B + And + nameof(AValidHost) + And + nameof(VerboseOutputIsSpecified) + And + nameof(TheRunCommand) + And + nameof(InputFolderIs) + A)]
        public void Scenario5() =>
            this.Given(x => x.KongMatchesInputFolder(B))
                .And(x => x.AValidHost())
                .And(x => x.VerboseOutputIsSpecified())
                .And(x => x.TheRunCommand())
                .And(x => x.InputFolderIs(A))
                .When(x => x.InvokingMain())
                .And(x => x.TheExitCodeIs(ExitCode.Success))
                .And(x => x.InvokingMainAgainForExport())
                .Then(x => x.TheExitCodeIs(ExitCode.Success))
                .And(x => x.OutputFolderContentsMatchInputFolderContents())
                .TearDownWith(s => KongIsBlank())
                .BDDfy();

        [BddfyFact(DisplayName = nameof(KongMatchesInputFolder) + A + And + nameof(AValidHost) + And + nameof(TheRunCommand) + And + nameof(InputFolderIs) + A)]
        public void Scenario6() =>
            this.Given(x => x.KongMatchesInputFolder(A))
                .And(x => x.AValidHost())
                .And(x => x.TheRunCommand())
                .And(x => x.InputFolderIs(A))
                .When(x => x.InvokingMain())
                .And(x => x.TheExitCodeIs(ExitCode.Success))
                .And(x => x.InvokingMainAgainForExport())
                .Then(x => x.TheExitCodeIs(ExitCode.Success))
                .And(x => x.OutputFolderContentsMatchInputFolderContents())
                .TearDownWith(s => KongIsBlank())
                .BDDfy();

        [BddfyFact(DisplayName = nameof(AValidHost) + And + nameof(AValidUser) + And + nameof(AValidPasswordFromOptions) + And + nameof(TheExportCommand) + And + nameof(OutputFolder) + Output)]
        public void Scenario7() =>
            this.Given(x => x.AValidHost())
                .And(x => x.AValidUser())
                .And(x => x.AValidPasswordFromOptions())
                .And(x => x.TheExportCommand())
                .And(x => x.OutputFolderIs(Output))
                .When(x => x.InvokingMain())
                .Then(x => x.TheExitCodeIs(ExitCode.Success))
                .And(x => x.TheAuthenticationHeaderIsSet())
                .BDDfy();

        [BddfyFact(DisplayName = nameof(AValidHost) + And + nameof(AValidUser) + And + nameof(AValidPasswordFromRedirectedStdIn) + And + nameof(TheExportCommand) + And + nameof(OutputFolder) + Output)]
        public void Scenario8() =>
            this.Given(x => x.AValidHost())
                .And(x => x.AValidUser())
                .And(x => x.AValidPasswordFromRedirectedStdIn())
                .And(x => x.TheExportCommand())
                .And(x => x.OutputFolderIs(Output))
                .When(x => x.InvokingMain())
                .Then(x => x.TheExitCodeIs(ExitCode.Success))
                .And(x => x.TheAuthenticationHeaderIsSet())
                .BDDfy();

    }
}
