using System.Threading.Tasks;
using Kongverge.DTOs;
using Kongverge.Helpers;
using Kongverge.Services;
using Microsoft.Extensions.Options;

namespace Kongverge.Workflow
{
    public class ExportWorkflow : Workflow
    {
        private readonly ConfigFileWriter _configWriter;
        private readonly ConfigBuilder _configBuilder;

        public ExportWorkflow(
            IKongAdminReader kongReader,
            IOptions<Settings> configuration,
            ConfigFileWriter configWriter,
            ConfigBuilder configBuilder) : base(kongReader, configuration)
        {
            _configWriter = configWriter;
            _configBuilder = configBuilder;
        }

        public override async Task<int> DoExecute()
        {
            var existingConfiguration = await _configBuilder.FromKong(KongReader);

            await _configWriter.WriteConfiguration(existingConfiguration, Configuration.OutputFolder);

            return ExitWithCode.Return(ExitCode.Success);
        }
    }
}
