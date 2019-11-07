using System.Threading.Tasks;
using Kongverge.DTOs;
using Kongverge.Helpers;
using Kongverge.Services;

namespace Kongverge.Workflow
{
    public class ExportWorkflow : Workflow
    {
        private readonly ExportWorkflowArguments _arguments;
        private readonly IConfigFileWriter _configWriter;
        private readonly ConfigBuilder _configBuilder;

        public ExportWorkflow(
            IKongAdminReader kongReader,
            ExportWorkflowArguments arguments,
            IConfigFileWriter configWriter,
            ConfigBuilder configBuilder) : base(kongReader)
        {
            _arguments = arguments;
            _configWriter = configWriter;
            _configBuilder = configBuilder;
        }

        public override async Task<int> DoExecute()
        {
            var existingConfiguration = await _configBuilder.FromKong(KongReader, _arguments.IgnoreTags);

            await _configWriter.WriteConfiguration(existingConfiguration, _arguments.OutputFolder);

            return ExitWithCode.Return(ExitCode.Success);
        }
    }
}
