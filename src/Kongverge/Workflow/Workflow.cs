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

        public async Task<int> Execute()
        {
            var reachable = await KongReader.KongIsReachable();
            if (!reachable)
            {
                return ExitWithCode.Return(ExitCode.HostUnreachable);
            }

            return await DoExecute();
        }

        public abstract Task<int> DoExecute();
    }
}
