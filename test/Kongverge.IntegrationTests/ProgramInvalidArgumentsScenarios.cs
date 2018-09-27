using Kongverge.Helpers;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;

namespace Kongverge.IntegrationTests
{
    public class ProgramInvalidArgumentsScenarios : ProgramSteps
    {
        [BddfyFact(DisplayName = nameof(NoArguments))]
        public void Scenario1() =>
            this.Given(x => x.NoArguments())
                .When(x => x.InvokingMain())
                .Then(x => x.TheExitCodeIs(ExitCode.MissingHost))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(AValidHost) + And + nameof(NoPort) + And + nameof(NoInputOrOutputFolder))]
        public void Scenario2() =>
            this.Given(x => x.AValidHost())
                .And(x => NoPort())
                .And(x => NoInputOrOutputFolder())
                .When(x => x.InvokingMain())
                .Then(x => x.TheExitCodeIs(ExitCode.MissingPort))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(AValidHost) + And + nameof(AnInvalidPort) + And + nameof(NoInputOrOutputFolder))]
        public void Scenario3() =>
            this.Given(x => x.AValidHost())
                .And(x => x.AnInvalidPort())
                .And(x => NoInputOrOutputFolder())
                .When(x => x.InvokingMain())
                .Then(x => x.TheExitCodeIs(ExitCode.InvalidPort))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(AValidHost) + And + nameof(AValidPort) + And + nameof(NoInputOrOutputFolder))]
        public void Scenario4() =>
            this.Given(x => x.AValidHost())
                .And(x => x.AValidPort())
                .And(x => x.NoInputOrOutputFolder())
                .When(x => x.InvokingMain())
                .Then(x => x.TheExitCodeIs(ExitCode.IncompatibleArguments))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(AValidHost) + And + nameof(AValidPort) + And + nameof(InputAndOutputFolders))]
        public void Scenario5() =>
            this.Given(x => x.AValidHost())
                .And(x => x.AValidPort())
                .And(x => x.InputAndOutputFolders())
                .When(x => x.InvokingMain())
                .Then(x => x.TheExitCodeIs(ExitCode.IncompatibleArguments))
                .BDDfy();
    }
}
