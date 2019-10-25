using System.Threading.Tasks;
using Kongverge.Helpers;
using Kongverge.Services;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;

namespace Kongverge.Tests.Workflow
{
    [Story(Title = nameof(TestWorkflow) + nameof(TestWorkflow.DoExecute))]
    public class CommonWorkflowScenarios : WorkflowSteps<TestWorkflow>
    {
        protected string KongVersion;

        public CommonWorkflowScenarios()
        {
            Plugins = Fixture.CreatePlugins(1);
        }

        [BddfyFact(DisplayName = nameof(ExitCode.HostVersionNotSupported))]
        public void Scenario1() =>
            this.Given(s => s.KongIsReachable(KongVersion))
                .When(s => s.Executing())
                .Then(s => s.TheExitCodeIs(ExitCode.HostVersionNotSupported))
                .WithExamples(new ExampleTable(nameof(KongVersion))
                {
                    { "0.35-0-enterprise-edition" },
                    { "0.35-2-enterprise-edition" },
                    { "0.36-1-enterprise-edition" },
                    { "0.36-2-enterprise-edition" },
                    { "1.0.0" },
                    { "1.1.0" },
                    { "1.2.0" },
                    { "1.4.0" }
                })
                .BDDfy();

        [BddfyFact(DisplayName = "HostVersionSupported")]
        public void Scenario2() =>
            this.Given(s => s.KongIsReachable(KongVersion))
                .When(s => s.Executing())
                .Then(s => s.TheExitCodeIs(ExitCode.Success))
                .WithExamples(new ExampleTable(nameof(KongVersion))
                {
                    { "0.34-1-enterprise-edition" },
                    { "0.33-1-enterprise-edition" },
                    { "0.32-1-enterprise-edition" },
                    { "0.31-0-enterprise-edition" },
                    { "0.14.999" },
                    { "0.13.1" },
                    { "0.12.1" },
                    { "0.11.0" }
                })
                .BDDfy();
    }

    public class TestWorkflow : Kongverge.Workflow.Workflow
    {
        public TestWorkflow(IKongAdminReader kongReader) : base(kongReader) { }

        public override Task<int> DoExecute() => Task.FromResult(ExitWithCode.Return(ExitCode.Success));
    }
}
