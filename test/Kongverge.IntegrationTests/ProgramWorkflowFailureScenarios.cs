using Kongverge.Helpers;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace Kongverge.IntegrationTests
{
    [Collection(Host)]
    public class ProgramWorkflowFailureScenarios : ProgramSteps
    {
        [BddfyFact(DisplayName = nameof(AValidHost) + And + nameof(AValidPort) + And + nameof(InputFolderIs) + NonExistent)]
        public void Scenario1() =>
            this.Given(x => x.AValidHost())
                .And(x => x.AValidPort())
                .And(x => x.InputFolderIs(NonExistent))
                .When(x => x.InvokingMain())
                .Then(x => x.TheExitCodeIs(ExitCode.InputFolderUnreachable))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(AValidHost) + And + nameof(AValidPort) + And + nameof(InputFolderIs) + InvalidData1)]
        public void Scenario2() =>
            this.Given(x => x.AValidHost())
                .And(x => x.AValidPort())
                .And(x => x.InputFolderIs(InvalidData1))
                .When(x => x.InvokingMain())
                .Then(x => x.TheExitCodeIs(ExitCode.InvalidConfigurationFile))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(AValidHost) + And + nameof(AValidPort) + And + nameof(InputFolderIs) + InvalidData2)]
        public void Scenario3() =>
            this.Given(x => x.AValidHost())
                .And(x => x.AValidPort())
                .And(x => x.InputFolderIs(InvalidData2))
                .When(x => x.InvokingMain())
                .Then(x => x.TheExitCodeIs(ExitCode.InvalidConfigurationFile))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(AValidHost) + And + nameof(AValidPort) + And + nameof(InputFolderIs) + BadFormat)]
        public void Scenario4() =>
            this.Given(x => x.AValidHost())
                .And(x => x.AValidPort())
                .And(x => x.InputFolderIs(BadFormat))
                .When(x => x.InvokingMain())
                .Then(x => x.TheExitCodeIs(ExitCode.InvalidConfigurationFile))
                .BDDfy();
    }
}
