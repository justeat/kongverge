using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Kongverge.DTOs;
using Kongverge.Helpers;
using Kongverge.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace Kongverge.Tests.Workflow
{
    public abstract class WorkflowSteps<T> : ScenarioFor<T> where T : Kongverge.Workflow.Workflow
    {
        protected KongvergeConfiguration Existing = new KongvergeConfiguration();
        protected KongConfiguration KongConfiguration;
        protected Settings Settings;
        protected ExitCode ExitCode;

        protected WorkflowSteps()
        {
            Settings = Fixture.Create<Settings>();
            GetMock<IOptions<Settings>>().Setup(x => x.Value).Returns(Settings);
            GetMock<ConfigBuilder>().Setup(x => x.FromKong(Get<IKongAdminReader>())).ReturnsAsync(Existing);
        }

        protected void KongIsNotReachable() => GetMock<IKongAdminReader>().Setup(x => x.GetConfiguration()).Throws<HttpRequestException>();

        protected void KongIsReachable() => GetMock<IKongAdminReader>().Setup(x => x.GetConfiguration()).ReturnsAsync(KongConfiguration = Fixture.Create<KongConfiguration>());

        protected async Task Executing() => ExitCode = (ExitCode)await Subject.Execute();

        protected void TheExitCodeIs(ExitCode exitCode) => ExitCode.Should().Be(exitCode);

        protected void TheConfigurationIsRetrievedFromKong() => GetMock<ConfigBuilder>().Verify(x => x.FromKong(Get<IKongAdminReader>()));
    }
}
