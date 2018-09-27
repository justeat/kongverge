using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Kongverge.Helpers;
using Kongverge.Services;

namespace Kongverge.IntegrationTests
{
    public abstract class ProgramSteps
    {
        public const string Host = "localhost";
        public const int Port = 8001;

        protected const string And = "_";
        protected const string NonExistent = nameof(NonExistent);
        protected const string InvalidData1 = nameof(InvalidData1);
        protected const string InvalidData2 = nameof(InvalidData2);
        protected const string BadFormat = nameof(BadFormat);
        protected const string A = nameof(A);
        protected const string B = nameof(B);
        protected const string Output = nameof(Output);

        protected CommandLineArguments Arguments = new CommandLineArguments();
        protected string InputFolder;
        protected string OutputFolder;
        protected ExitCode ExitCode;

        private static string MakeFolderName(string name) => Path.IsPathRooted(name) ? name : $"Folder{name}";

        protected void InvokingMain() => ExitCode = (ExitCode)Program.Main(Arguments.ToArray());

        protected void InvokingMainAgainForExport()
        {
            TheExitCodeIs(ExitCode.Success);
            Arguments = new CommandLineArguments();
            AValidHost();
            AValidPort();
            OutputFolderIs(Output);
            ExitCode = (ExitCode)Program.Main(Arguments.ToArray());
        }

        protected void NoArguments() { }

        protected void AnInvalidPort() => Arguments.AddPair("--port", 1);

        protected void AValidHost() => Arguments.AddPair("--host", Host);

        protected void AValidPort() => Arguments.AddPair("--port", Port);

        protected void NoPort() { }

        protected void NoInputOrOutputFolder() { }

        protected void InputAndOutputFolders()
        {
            InputFolderIs(Guid.NewGuid().ToString());
            OutputFolderIs(Guid.NewGuid().ToString());
        }

        protected void InputFolderIs(string name)
        {
            InputFolder = MakeFolderName(name);
            Arguments.AddPair("--input", InputFolder);
        }

        protected void OutputFolderIs(string name)
        {
            OutputFolder = MakeFolderName(name);
            Arguments.AddPair("--output", OutputFolder);
        }

        protected void KongIsBlank()
        {
            var tempPath = Path.GetTempPath();
            var folderPath = Path.Join(tempPath, GetType().Namespace);
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath);
            }

            Directory.CreateDirectory(folderPath);
            KongMatchesInputFolder(folderPath);
        }

        protected void KongMatchesInputFolder(string folder)
        {
            Arguments = new CommandLineArguments();
            AValidHost();
            AValidPort();
            InputFolderIs(folder);
            ExitCode = (ExitCode)Program.Main(Arguments.ToArray());
            TheExitCodeIs(ExitCode.Success);
            Arguments = new CommandLineArguments();
        }

        protected void TheExitCodeIs(ExitCode exitCode) => ExitCode.Should().Be(exitCode);

        protected async Task OutputFolderContentsMatchInputFolderContents()
        {
            Debug.WriteLine(Directory.GetCurrentDirectory());

            var configReader = new ConfigFileReader();
            var inputConfiguration = await configReader.ReadConfiguration(InputFolder);
            var outputConfiguration = await configReader.ReadConfiguration(OutputFolder);

            inputConfiguration.GlobalConfig.Plugins.Should().NotBeEmpty();
            inputConfiguration.Services.Count.Should().Be(3);

            outputConfiguration.GlobalConfig.Plugins.Should().BeEquivalentTo(inputConfiguration.GlobalConfig.Plugins);
            outputConfiguration.Services.Should().BeEquivalentTo(inputConfiguration.Services);
            foreach (var outputService in outputConfiguration.Services)
            {
                var inputService = inputConfiguration.Services.Single(x => x.Name == outputService.Name);
                outputService.Plugins.Should().BeEquivalentTo(inputService.Plugins);
                outputService.Routes.Should().BeEquivalentTo(inputService.Routes);
                foreach (var outputServiceRoute in outputService.Routes)
                {
                    var inputServiceRoute = inputService.Routes.Single(x => x.Equals(outputServiceRoute));
                    outputServiceRoute.Plugins.Should().BeEquivalentTo(inputServiceRoute.Plugins);
                }
            }
        }
    }

    public class CommandLineArguments : List<string>
    {
        public void AddPair(string name, object value)
        {
            Add(name);
            Add(value.ToString());
        }
    }
}
