using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using Xunit;

namespace Kongverge.IntegrationTests
{
    [CollectionDefinition(ProgramSteps.Host, DisableParallelization = true)]
    public class DockerServicesStartupDelay : ICollectionFixture<DockerServicesStartupDelay>
    {
        public DockerServicesStartupDelay()
        {
            try
            {
                if (bool.TryParse(Environment.GetEnvironmentVariable("APPVEYOR"), out var isAppVeyor) && isAppVeyor)
                {
                    // For some unknown reason, we have to do this for AppVeyor builds, even though we are already
                    // starting docker-compose in the test scripts defined in our "appveyor.yml" file. We have to do
                    // this in addition to that (neither one on it's own works). Very strange!

                    // ReSharper disable once PossibleNullReferenceException
                    Process.Start("docker-compose", "start").WaitForExit();
                }
                WaitForKong();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected static void WaitForKong()
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri($"http://{ProgramSteps.Host}:{ProgramSteps.Port}"),
                Timeout = TimeSpan.FromMilliseconds(300)
            };
            using (httpClient)
            {
                var count = 0;
                while (true)
                {
                    try
                    {
                        var response = httpClient.GetAsync("/").Result;
                        response.EnsureSuccessStatusCode();
                        return;
                    }
                    catch
                    {
                        if (++count >= 10)
                        {
                            throw;
                        }
                        Thread.Sleep(700);
                    }
                }
            }
        }
    }
}
