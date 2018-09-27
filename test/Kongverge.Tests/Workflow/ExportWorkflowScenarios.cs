using Kongverge.Helpers;
using Kongverge.Services;
using Kongverge.Workflow;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;

namespace Kongverge.Tests.Workflow
{
    public class ExportWorkflowScenarios : WorkflowSteps<ExportWorkflow>
    {
        [BddfyFact(DisplayName = nameof(KongIsNotReachable))]
        public void Scenario1() =>
            this.Given(s => s.KongIsNotReachable())
                .When(s => s.Executing())
                .Then(s => s.TheExitCodeIs(ExitCode.HostUnreachable))
                .BDDfy();

        [BddfyFact(DisplayName = nameof(KongIsReachable))]
        public void Scenario2() =>
            this.Given(s => s.KongIsReachable())
                .When(s => s.Executing())
                .Then(s => s.TheConfigurationIsRetrievedFromKong())
                .And(s => s.TheConfigurationIsWrittenToOutputFolder())
                .And(s => s.TheExitCodeIs(ExitCode.Success))
                .BDDfy();

        protected void TheConfigurationIsWrittenToOutputFolder() =>
            GetMock<ConfigFileWriter>().Verify(x => x.WriteConfiguration(Existing, Settings.OutputFolder));
    }
}
