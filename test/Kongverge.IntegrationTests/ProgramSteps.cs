using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Kongverge.DTOs;
using Kongverge.Helpers;
using Kongverge.Services;
using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx;

namespace Kongverge.IntegrationTests
{
    public abstract class ProgramSteps
    {
        public const string Host = "localhost";
        public const int Port = 8001;

        protected const string And = "_";
        protected const string NonExistent = nameof(NonExistent);
        protected const string InvalidData = nameof(InvalidData);
        protected const string A = nameof(A);
        protected const string B = nameof(B);
        protected const string Output = nameof(Output);
        protected const string User = nameof(User);
        protected const string Password = nameof(Password);

        protected CommandLineArguments Arguments = new CommandLineArguments();
        protected string InputFolder;
        protected string OutputFolder;
        protected ExitCode ExitCode;
        protected FakeConsole Console;

        protected ProgramSteps()
        {
            Program.Console = Console = new FakeConsole
            {
                Out = new StringWriter(new StringBuilder()),
                Error = new StringWriter(new StringBuilder())
            };
            Program.HttpMessageHandler = new HttpClientHandler();
        }

        private static string MakeFolderName(string name) => Path.IsPathRooted(name) ? name : $"Folder{name}";

        protected void InvokingMain()
        {
            var arguments = Arguments.ToArray();
            Arguments.Clear();
            ExitCode = (ExitCode)Program.Main(arguments);
        }

        protected void InvokingMainAgainForExport()
        {
            AValidHost();
            TheExportCommand();
            OutputFolderIs(Output);
            InvokingMain();
        }

        protected void NoArguments() { }

        protected void AnInvalidPort() => Arguments.AddPair("--port", 1);

        protected void AValidHost() => Arguments.AddPair("--host", Host);

        protected void TheRunCommand() => Arguments.Add("run");

        protected void TheDryRunCommand() => Arguments.Add("dry-run");

        protected void TheExportCommand() => Arguments.Add("export");

        protected void VerboseOutputIsSpecified() => Arguments.Add("--verbose");

        protected void AValidUser() => Arguments.AddPair("--user", User);

        protected void AValidPasswordFromOptions() => Arguments.Add($"--password={Password}");

        protected void AValidPasswordFromRedirectedStdIn()
        {
            Arguments.Add("--password");
            Console.IsInputRedirected = true;
            Console.In = new StringReader(Password);
        }

        protected void AnInvalidInputFolder() => Arguments.Add(NonExistent);

        protected void NoUser() { }

        protected void NoPassword() { }

        protected void NoCommand() { }

        protected void NoInputFolder() { }

        protected void NoOutputFolder() { }

        protected void AnUnrecognizedArgument(string argument) => Arguments.Add(argument);

        protected void InputFolderIs(string name)
        {
            InputFolder = MakeFolderName(name);
            Arguments.Add(InputFolder);
        }

        protected void OutputFolderIs(string name)
        {
            OutputFolder = MakeFolderName(name);
            Arguments.Add(OutputFolder);
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
            AValidHost();
            TheRunCommand();
            InputFolderIs(folder);
            InvokingMain();
            TheExitCodeIs(ExitCode.Success);
        }

        protected void TheAuthenticationHeaderIsSet()
        {
            var connectionDetails = Program.ServiceProvider.GetRequiredService<KongAdminApiConnectionDetails>();
            connectionDetails.AuthenticationHeader.Should().NotBeNull();
            connectionDetails.AuthenticationHeader.Scheme.Should().Be("Basic");
            var decodedBytes = Convert.FromBase64String(connectionDetails.AuthenticationHeader.Parameter);
            var decodedString = Encoding.ASCII.GetString(decodedBytes);
            decodedString.Should().Be($"{User}:{Password}");
        }

        protected void KongRespondsWithBadRequestAfterPartiallyApplyingNewConfiguration()
        {
            var mutationRequestCount = 0;
            Program.HttpMessageHandler = new FakeHttpMessageHandler((handler, request, cancellationToken) =>
            {
                if (request.Method == HttpMethod.Delete || request.Method == HttpMethod.Patch || request.Method == HttpMethod.Post || request.Method == HttpMethod.Put)
                {
                    if (++mutationRequestCount == 5)
                    {
                        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("Error") });
                    }
                }

                return handler.NormalSendAsync(request, cancellationToken);
            });
        }

        protected void TheExitCodeIs(ExitCode exitCode) => ExitCode.Should().Be(exitCode);

        protected void AnErrorMessageIsShownContaining(params string[] messages)
        {
            foreach (var message in messages)
            {
                Console.Error.ToString().Should().Contain(message);
            }
        }

        protected void TheHelpTextIsShownContaining(string message) => Console.Out.ToString().Should().Contain(message);

        protected Task OutputFolderContentsMatchInputFolderContents() => OutputFolderContentsMatchesFolderContentsOf(InputFolder);

        protected async Task OutputFolderContentsMatchesFolderContentsOf(string name)
        {
            var folder = name.StartsWith("Folder") ? name : MakeFolderName(name);

            Debug.WriteLine(Directory.GetCurrentDirectory());

            var kongReader = new KongAdminReader(new KongAdminHttpClient(new KongAdminApiConnectionDetails()));
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
