using System;
using System.Net.Http;
using System.Threading.Tasks;
using Kongverge.DTOs;
using Kongverge.Helpers;
using Kongverge.Services;

namespace Kongverge.Workflow
{
    public abstract class Workflow
    {
        protected Workflow(IKongAdminReader kongReader)
        {
            KongReader = kongReader;
        }

        protected IKongAdminReader KongReader { get; }
        protected KongConfiguration KongConfiguration { get; private set; }

        public async Task<int> Execute()
        {
            try
            {
                KongConfiguration = await KongReader.GetConfiguration();
            }
            catch (Exception e) when (e is KongException || e is HttpRequestException)
            {
                return ExitWithCode.Return(ExitCode.HostUnreachable, e.Message);
            }

            return await DoExecute();
        }

        public abstract Task<int> DoExecute();
    }
}
