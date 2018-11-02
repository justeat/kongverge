using System.Threading.Tasks;
using Kongverge.DTOs;
using Kongverge.Helpers;
using Kongverge.Services;
using Microsoft.Extensions.Options;

namespace Kongverge.Workflow
{
    public abstract class Workflow
    {
        protected Workflow(IKongAdminReader kongReader, IOptions<Settings> configuration)
        {
            KongReader = kongReader;
            Configuration = configuration.Value;
        }

        protected IKongAdminReader KongReader { get; }
        protected Settings Configuration { get; }
        protected KongConfiguration KongConfiguration { get; private set; }

        public async Task<int> Execute()
        {
            try
            {
                KongConfiguration = await KongReader.GetConfiguration();
            }
            catch
            {
                return ExitWithCode.Return(ExitCode.HostUnreachable);
            }

            return await DoExecute();
        }

        public abstract Task<int> DoExecute();
    }
}
