using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Kongverge.DTOs;
using Kongverge.Helpers;
using Kongverge.Services;
using Microsoft.Extensions.Options;
using Nito.AsyncEx;

namespace Kongverge.IntegrationTests
{
    public abstract class ProgramSteps
    {
        public const string Host = "localhost";
        public const int Port = 8001;

        protected const string And = "_";
        protected const string NonExistent = nameof(NonExistent);
        protected const string InvalidDataA = nameof(InvalidDataA);
        protected const string InvalidDataB = nameof(InvalidDataB);
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
            TheExitCodeIs(InputFolder.Contains(InvalidDataB) ? ExitCode.InvalidConfigurationFiles : ExitCode.Success);
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

        protected void VerboseOutput() => Arguments.Add("--verbose");

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

        protected Task OutputFolderContentsMatchInputFolderContents() => OutputFolderContentsMatchesFolderContentsOf(InputFolder);

        protected async Task OutputFolderContentsMatchesFolderContentsOf(string name)
        {
            var folder = name.StartsWith("Folder") ? name : MakeFolderName(name);

            Debug.WriteLine(Directory.GetCurrentDirectory());

            var settings = new Settings
            {
                Admin = new Admin { Host = Host, Port = Port }
            };
            var kongReader = new KongAdminReader(new KongAdminHttpClient(Options.Create(settings)));
            var kongConfiguration = await kongReader.GetConfiguration();
            var availablePlugins = kongConfiguration.Plugins.Available.Where(x => x.Value).Select(x => x.Key).ToDictionary(x => x, x => new AsyncLazy<KongPluginSchema>(() => kongReader.GetPluginSchema(x)));
            var configReader = new ConfigFileReader();
            var folderConfiguration = await configReader.ReadConfiguration(folder, availablePlugins);
            var outputConfiguration = await configReader.ReadConfiguration(OutputFolder, availablePlugins);

            folderConfiguration.GlobalConfig.Plugins.Should().NotBeEmpty();
            folderConfiguration.Services.Count.Should().Be(3);

            outputConfiguration.GlobalConfig.Plugins.Should().BeEquivalentTo(folderConfiguration.GlobalConfig.Plugins);
            outputConfiguration.Services.Should().BeEquivalentTo(folderConfiguration.Services);
            foreach (var outputService in outputConfiguration.Services)
            {
                var folderService = folderConfiguration.Services.Single(x => x.Name == outputService.Name);
                outputService.Plugins.Should().BeEquivalentTo(folderService.Plugins);
                outputService.Routes.Should().BeEquivalentTo(folderService.Routes);
                foreach (var outputServiceRoute in outputService.Routes)
                {
                    var folderServiceRoute = folderService.Routes.Single(x => x.Equals(outputServiceRoute));
                    outputServiceRoute.Plugins.Should().BeEquivalentTo(folderServiceRoute.Plugins);
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
