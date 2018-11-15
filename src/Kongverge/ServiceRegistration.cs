using Kongverge.DTOs;
using Kongverge.Services;
using Kongverge.Workflow;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;

namespace Kongverge
{
    public static class ServiceRegistration
    {
        public static void CreateConsoleLogger(bool verbose)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(verbose ? LogEventLevel.Verbose : LogEventLevel.Information)
                .WriteTo.Console()
                .CreateLogger();
        }

        public static IServiceCollection ConfigureServices(this IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            services.Configure<Settings>(x => configuration.Bind(x));

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
                var config = s.GetService<IOptions<Settings>>();

                if (config.Value.DryRun)
                {
                    Log.Information("Performing dry run: No writes to Kong will occur");
                    return s.GetService<KongAdminDryRun>();
                }

                Log.Information($"Performing live integration: Changes will be made to {config.Value.Admin.Host}");
                return s.GetService<KongAdminWriter>();
            });

            services.AddTransient<Workflow.Workflow>(s =>
            {
                var config = s.GetService<IOptions<Settings>>();

                if (string.IsNullOrEmpty(config.Value.OutputFolder))
                {
                    Log.Information("Performing full diff and merge");
                    return s.GetService<KongvergeWorkflow>();
                }

                Log.Information("Exporting configuration from Kong");
                return s.GetService<ExportWorkflow>();
            });

            return services;
        }
    }
}
