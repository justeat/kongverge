using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Kongverge.DTOs;
using Kongverge.Helpers;
using Kongverge.Services;
using Moq;

namespace Kongverge.Tests.Workflow
{
    public abstract class WorkflowSteps<T> : ScenarioFor<T> where T : Kongverge.Workflow.Workflow
    {
        protected KongvergeConfiguration Existing = new KongvergeConfiguration();
        protected KongConfiguration KongConfiguration;
        protected ExitCode ExitCode;
        
        protected IReadOnlyList<KongPlugin> Plugins;

        protected WorkflowSteps()
        {
            GetMock<ConfigBuilder>().Setup(x => x.FromKong(Get<IKongAdminReader>())).ReturnsAsync(Existing);
            GetMock<IKongAdminReader>().Setup(x => x.GetPluginSchema(It.IsAny<string>())).ReturnsAsync(new KongPluginSchema { Fields = new Dictionary<string, FieldDefinition>() });
        }

        protected void KongIsNotReachable() => GetMock<IKongAdminReader>().Setup(x => x.GetConfiguration()).Throws<HttpRequestException>();

        protected void KongIsReachable() => GetMock<IKongAdminReader>()
            .Setup(x => x.GetConfiguration())
            .ReturnsAsync(KongConfiguration = Fixture.Build<KongConfiguration>().With(x => x.Plugins, new Plugins { Available = Plugins.ToDictionary(x => x.Name, x => true) }).Create());

        protected async Task Executing() => ExitCode = (ExitCode)await Subject.Execute();

        protected void TheExitCodeIs(ExitCode exitCode) => ExitCode.Should().Be(exitCode);

        protected void TheConfigurationIsRetrievedFromKong() => GetMock<ConfigBuilder>().Verify(x => x.FromKong(Get<IKongAdminReader>()));
    }
}
