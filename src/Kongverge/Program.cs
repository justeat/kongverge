using System;
using Kongverge.DTOs;
using Kongverge.Helpers;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;

namespace Kongverge
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var app = new CommandLineApplication
            {
                Name = "Kongverge",
                Description = "Kong configuration convergence."
            };

            var options = new Options(app);

            app.OnExecute(async () =>
            {
                ServiceRegistration.CreateConsoleLogger();

                var exitCode = options.Validate();
                if (exitCode.HasValue)
                {
                    return ExitWithCode.Return(exitCode.Value);
                }

                var serviceProvider = new ServiceCollection().ConfigureServices().BuildServiceProvider();

                options.Apply(serviceProvider);

                var workflow = serviceProvider.GetService<Workflow.Workflow>();

                Log.Information($"************** {app.Name} **************");

                return await workflow.Execute();
            });

            try
            {
                return app.Execute(args);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error running program: {message}", ex.Message);
                return (int)ExitCode.UnspecifiedError;
            }
        }

        private class Options
        {
            private const int MinPort = 1024;
            private const int MaxPort = 49151;

            public Options(CommandLineApplication app)
            {
                app.HelpOption("-?|-h|--help");

                DryRun = app.Option("-t|--test",
                    "Perform dry run without updating Kong system",
                    CommandOptionType.NoValue);

                InputFolder = app.Option("-i|--input <inputFolder>",
                    "Folder for input data",
                    CommandOptionType.SingleValue);

                OutputFolder = app.Option("-o|--output <outputFolder>",
                    "Folder to output data from host",
                    CommandOptionType.SingleValue);

                Host = app.Option("-H|--host <KongAdminHostname>",
                    "Kong Admin host with which to communicate",
                    CommandOptionType.SingleValue);

                Port = app.Option("-p|--port <KongAdminPort>",
                    "Kong Admin API port",
                    CommandOptionType.SingleValue);
            }

            public CommandOption DryRun { get; }
            public CommandOption InputFolder { get; }
            public CommandOption OutputFolder { get; }
            public CommandOption Host { get; }
            public CommandOption Port { get; }

            public ExitCode? Validate()
            {
                if (!Host.HasValue())
                {
                    return ExitCode.MissingHost;
                }

                if (!Port.HasValue())
                {
                    return ExitCode.MissingPort;
                }
                
                if (!int.TryParse(Port.Value(), out var port) || port > MaxPort || port < MinPort)
                {
                    return ExitCode.InvalidPort;
                }

                if (InputFolder.HasValue() && OutputFolder.HasValue())
                {
                    return ExitCode.IncompatibleArguments;
                }

                if (!InputFolder.HasValue() && !OutputFolder.HasValue())
                {
                    return ExitCode.IncompatibleArguments;
                }

                return null;
            }

            public void Apply(IServiceProvider serviceProvider)
            {
                var settings = serviceProvider.GetService<IOptions<Settings>>().Value;

                if (Host.HasValue())
                {
                    settings.Admin.Host = Host.Value();
                }

                if (OutputFolder.HasValue())
                {
                    settings.OutputFolder = OutputFolder.Value();
                }

                if (Port.HasValue())
                {
                    settings.Admin.Port = int.Parse(Port.Value());
                }

                settings.DryRun = DryRun.HasValue();

                if (InputFolder.HasValue())
                {
                    settings.InputFolder = InputFolder.Value();
                }
            }
        }
    }
}
