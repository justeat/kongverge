using System;
using System.Diagnostics;
using System.Threading;
using Xunit;

namespace Kongverge.IntegrationTests
{
    [CollectionDefinition(ProgramSteps.Host, DisableParallelization = true)]
    public class DockerServicesLifecycle : IDisposable, ICollectionFixture<DockerServicesLifecycle>
    {
        public DockerServicesLifecycle()
        {
            //try
            //{
            //    // ReSharper disable once PossibleNullReferenceException
            //    Process.Start("docker-compose", "start").WaitForExit();
            //    Thread.Sleep(3000);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e);
            //}
        }

        public void Dispose()
        {
            //try
            //{
            //    // ReSharper disable once PossibleNullReferenceException
            //    Process.Start("docker-compose", "stop").WaitForExit();
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e);
            //}
        }
    }
}
