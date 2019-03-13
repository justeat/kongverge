using System;
using System.Net.Http;
using Kongverge.DTOs;
using Kongverge.Helpers;
using Kongverge.Services;
using Kongverge.Workflow;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace Kongverge
{
    public class Program
    {
        public static IConsole Console = PhysicalConsole.Singleton;

        public static HttpMessageHandler HttpMessageHandler = new HttpClientHandler();

        public static IServiceProvider ServiceProvider { get; private set; }

        public static int Main(string[] args)
        {
            CreateLogger(true);

            var app = new CommandLineApplication<KongvergeCommand>(Console);
            Password.RegisterValueParser(app, Console);
            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(ServiceProvider = BuildServiceProvider());

            return app.ExecuteWithErrorHandling(args);
        }

        public static void CreateLogger(bool verbose)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(verbose ? LogEventLevel.Verbose : LogEventLevel.Information)
                .WriteTo.Console()
                .CreateLogger();
        }

        public static IServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddSingleton(Console);
            services.AddSingleton(HttpMessageHandler);
            services.AddSingleton<KongAdminApiConnectionDetails>();
            services.AddSingleton<KongvergeWorkflowArguments>();
            services.AddSingleton<ExportWorkflowArguments>();

            services.AddTransient<ConfigFileReader>();
            services.AddTransient<ConfigFileWriter>();
            services.AddTransient<ConfigBuilder>();
            services.AddTransient<KongAdminHttpClient>();
            services.AddTransient<KongAdminDryRun>();
            services.AddTransient<KongAdminWriter>();
            services.AddTransient<KongvergeWorkflow>();
            services.AddTransient<ExportWorkflow>();
            services.AddTransient<IKongAdminReader, KongAdminReader>();
            services.AddTransient<IKongAdminWriter>(s =>
            {
                var args = s.GetRequiredService<KongvergeWorkflowArguments>();

                if (args.DryRun)
                {
                    Log.Information("Performing dry run: No writes to Kong will occur");
                    return s.GetRequiredService<KongAdminDryRun>();
                }

                var connectionDetails = s.GetRequiredService<KongAdminApiConnectionDetails>();

                Log.Information($"Performing live integration: Changes will be made to {connectionDetails.Host}");
                return s.GetRequiredService<KongAdminWriter>();
            });

            return services.BuildServiceProvider();
        }
    }
}
