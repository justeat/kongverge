using Kongverge.Helpers;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;

namespace Kongverge.IntegrationTests
{
    [Story(Title = nameof(Program) + nameof(Program.Main) + "InvalidArguments")]
    public class ProgramInvalidArgumentsScenarios : ProgramSteps
    {
        [BddfyFact(DisplayName = nameof(NoArguments))]
        public void Scenario1() =>
            this.Given(x => x.NoArguments())
                .When(x => x.InvokingMain())
                .Then(x => x.TheExitCodeIs(ExitCode.UnspecifiedError))
                .And(x => x.AnErrorMessageIsShownContaining("The --host field is required."))
                .And(x => x.TheHelpTextIsShownContaining("Specify --help for a list of available options and commands."))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(AValidHost) + And + nameof(NoCommand))]
        public void Scenario2() =>
            this.Given(x => x.AValidHost())
                .And(x => x.NoCommand())
                .When(x => x.InvokingMain())
                .Then(x => x.TheExitCodeIs(ExitCode.UnspecifiedError))
                .And(x => x.TheHelpTextIsShownContaining("Usage: kongverge [options] [command]"))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(AValidHost) + And + nameof(AnInvalidPort))]
        public void Scenario3() =>
            this.Given(x => x.AValidHost())
                .And(x => x.AnInvalidPort())
                .When(x => x.InvokingMain())
                .Then(x => x.TheExitCodeIs(ExitCode.UnspecifiedError))
                .And(x => x.AnErrorMessageIsShownContaining("The field --port must be between 1024 and 49151."))
                .BDDfy();

        protected string UnrecognizedArgument;
        protected string[] ErrorMessages;

        [BddfyFact(DisplayName = nameof(AValidHost) + And + nameof(AnUnrecognizedArgument))]
        public void Scenario4() =>
            this.Given(x => x.AValidHost())
                .And(x => x.AnUnrecognizedArgument(UnrecognizedArgument))
                .When(x => x.InvokingMain())
                .Then(x => x.TheExitCodeIs(ExitCode.UnspecifiedError))
                .And(x => x.TheHelpTextIsShownContaining("Specify --help for a list of available options and commands."))
                .And(x => x.AnErrorMessageIsShownContaining(ErrorMessages))
                .WithExamples(new ExampleTable(nameof(UnrecognizedArgument), nameof(ErrorMessages))
                {
                    { "--not-recognized", new[] { "Unrecognized option '--not-recognized'" } },
                    { "runs", new[] { "Unrecognized command or argument 'runs'", "Did you mean this?" } }
                })
                .BDDfy();

        [BddfyFact(DisplayName = nameof(AValidHost) + And + nameof(AValidUser) + And + nameof(NoPassword))]
        public void Scenario5() =>
            this.Given(x => x.AValidHost())
                .And(x => x.AValidUser())
                .And(x => x.NoPassword())
                .When(x => x.InvokingMain())
                .Then(x => x.TheExitCodeIs(ExitCode.UnspecifiedError))
                .And(x => x.AnErrorMessageIsShownContaining("User was provided but Password was not."))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(AValidHost) + And + nameof(NoUser) + And + nameof(AValidPasswordFromOptions))]
        public void Scenario6() =>
            this.Given(x => x.AValidHost())
                .And(x => x.NoUser())
                .And(x => x.AValidPasswordFromOptions())
                .When(x => x.InvokingMain())
                .Then(x => x.TheExitCodeIs(ExitCode.UnspecifiedError))
                .And(x => x.AnErrorMessageIsShownContaining("Password was provided but User was not."))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(AValidHost) + And + nameof(TheRunCommand) + And + nameof(NoInputFolder))]
        public void Scenario7() =>
            this.Given(x => x.AValidHost())
                .And(x => x.TheRunCommand())
                .And(x => x.NoInputFolder())
                .When(x => x.InvokingMain())
                .Then(x => x.TheExitCodeIs(ExitCode.UnspecifiedError))
                .And(x => x.AnErrorMessageIsShownContaining("The InputFolder field is required."))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(AValidHost) + And + nameof(TheDryRunCommand) + And + nameof(NoInputFolder))]
        public void Scenario8() =>
            this.Given(x => x.AValidHost())
                .And(x => x.TheDryRunCommand())
                .And(x => x.NoInputFolder())
                .When(x => x.InvokingMain())
                .Then(x => x.TheExitCodeIs(ExitCode.UnspecifiedError))
                .And(x => x.AnErrorMessageIsShownContaining("The InputFolder field is required."))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(AValidHost) + And + nameof(TheRunCommand) + And + nameof(AnInvalidInputFolder))]
        public void Scenario9() =>
            this.Given(x => x.AValidHost())
                .And(x => x.TheRunCommand())
                .And(x => x.AnInvalidInputFolder())
                .When(x => x.InvokingMain())
                .Then(x => x.TheExitCodeIs(ExitCode.UnspecifiedError))
                .And(x => x.AnErrorMessageIsShownContaining("The directory 'NonExistent' does not exist."))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(AValidHost) + And + nameof(TheDryRunCommand) + And + nameof(AnInvalidInputFolder))]
        public void Scenario10() =>
            this.Given(x => x.AValidHost())
                .And(x => x.TheDryRunCommand())
                .And(x => x.AnInvalidInputFolder())
                .When(x => x.InvokingMain())
                .Then(x => x.TheExitCodeIs(ExitCode.UnspecifiedError))
                .And(x => x.AnErrorMessageIsShownContaining("The directory 'NonExistent' does not exist."))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(AValidHost) + And + nameof(TheExportCommand) + And + nameof(NoOutputFolder))]
        public void Scenario11() =>
            this.Given(x => x.AValidHost())
                .And(x => x.TheExportCommand())
                .And(x => x.NoOutputFolder())
                .When(x => x.InvokingMain())
                .Then(x => x.TheExitCodeIs(ExitCode.UnspecifiedError))
                .And(x => x.AnErrorMessageIsShownContaining("The OutputFolder field is required."))
                .BDDfy();
    }
}
