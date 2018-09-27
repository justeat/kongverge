using Kongverge.DTOs;
using Kongverge.Services;
using Kongverge.Workflow;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;

namespace Kongverge
{
    public static class ServiceRegistration
    {
        public static void CreateConsoleLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
            Log.Information("Starting up");
        }

        public static IServiceCollection ConfigureServices(this IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            services.Configure<Settings>(x => configuration.Bind(x));

            services.AddSingleton<ConfigFileReader>();
            services.AddSingleton<ConfigFileWriter>();
            services.AddSingleton<ConfigBuilder>();
            services.AddSingleton<KongAdminHttpClient>();
            services.AddSingleton<KongAdminDryRun>();
            services.AddSingleton<KongAdminWriter>();
            services.AddSingleton<KongvergeWorkflow>();
            services.AddSingleton<ExportWorkflow>();

            services.AddSingleton<IKongAdminReader, KongAdminReader>();

            services.AddSingleton<IKongAdminWriter>(s =>
            {
                var config = s.GetService<IOptions<Settings>>();

                if (config.Value.DryRun)
                {
                    Log.Information("Performing dry run. No writes to Kong will occur.");
                    return s.GetService<KongAdminDryRun>();
                }

                Log.Information("Performing live integration. Changes will be made to {host}.", config.Value.Admin.Host);
                return s.GetService<KongAdminWriter>();
            });

            services.AddSingleton<Workflow.Workflow>(s =>
            {
                var config = s.GetService<IOptions<Settings>>();

                if (string.IsNullOrEmpty(config.Value.OutputFolder))
                {
                    Log.Information("Performing full diff and merge.");
                    return s.GetService<KongvergeWorkflow>();
                }

                Log.Information("Exporting information from Kong.");
                return s.GetService<ExportWorkflow>();
            });

            return services;
        }
    }
}
