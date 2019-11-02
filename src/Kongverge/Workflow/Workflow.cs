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

            var versionSubStr = KongConfiguration.Version.Substring(0, 4);
            var version = double.Parse(versionSubStr[3] == '.' ? versionSubStr.Substring(0, 3) : versionSubStr);
            if (KongConfiguration.Version.Contains("enterprise-edition"))
            {
                const double lowestSupportedVersion = 0.36;
                if (version < lowestSupportedVersion)
                {
                    return ExitWithCode.Return(ExitCode.HostVersionNotSupported,
                        $"This version of Kongverge can only support Kong enterprise from version {lowestSupportedVersion}.x");
                }
            }
            else
            {
                const double lowestSupportedVersion = 1.1;
                if (version < lowestSupportedVersion)
                {
                    return ExitWithCode.Return(ExitCode.HostVersionNotSupported,
                        $"This version of Kongverge can only support Kong from version {lowestSupportedVersion}.x");
                }
            }

            return await DoExecute();
        }

        public abstract Task<int> DoExecute();
    }
}
