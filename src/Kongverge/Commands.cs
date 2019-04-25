using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Kongverge.DTOs;
using Kongverge.Workflow;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Kongverge
{
    [Command("kongverge")]
    [Subcommand(typeof(DryRunCommand), typeof(RunCommand), typeof(ExportCommand))]
    public class KongvergeCommand : CommandBase
    {
        [Option("-v|--verbose", "Optional. Verbose console output", CommandOptionType.NoValue)]
        public bool Verbose { get; }

        [Required]
        [Option("-H|--host <KongAdminHost>", "Required. Kong Admin API host", CommandOptionType.SingleValue)]
        public string Host { get; }

        [Range(1024, 49151)]
        [Option("-P|--port <KongAdminPort>", "Optional. Kong Admin API port (defaults to 8001)", CommandOptionType.SingleValue)]
        public int Port { get; } = 8001;

        [Option("-u|--user <Username>", "Optional. Basic Auth protected Kong Admin API Username", CommandOptionType.SingleValue)]
        public string BasicAuthUser { get; }

        [Option("-p|--password=<Password>", "Optional. Basic Auth protected Kong Admin API Password (can be passed via redirected stdin)", CommandOptionType.SingleOrNoValue)]
        public Password BasicAuthPassword { get; }

        [Option("-ft|--faultTolerance <FaultTolerance>", "Optional. True or false (defaults to false). If true, allows Kongverge to complete a run through regardless of exceptions", CommandOptionType.SingleValue)]
        public bool FaultTolerance { get; } = false;

        protected ValidationResult OnValidate(ValidationContext validationContext, CommandLineContext commandLineContext)
        {
            if (!string.IsNullOrWhiteSpace(BasicAuthUser) && string.IsNullOrWhiteSpace(BasicAuthPassword.Value))
            {
                return new ValidationResult("User was provided but Password was not.", new[] { nameof(BasicAuthUser), nameof(BasicAuthPassword) });
            }
            if (!string.IsNullOrWhiteSpace(BasicAuthPassword.Value) && string.IsNullOrWhiteSpace(BasicAuthUser))
            {
                return new ValidationResult("Password was provided but User was not.", new[] { nameof(BasicAuthUser), nameof(BasicAuthPassword) });
            }

            return ValidationResult.Success;
        }

        public override void Initialize(CommandLineApplication app)
        {
            Program.CreateLogger(Verbose);

            var assembly = typeof(CommandBase).Assembly;
            var product = assembly.GetCustomAttribute<AssemblyProductAttribute>().Product;
            var version = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
            Log.Information($"************** {product} {version} **************");

            var connectionDetails = app.GetRequiredService<KongAdminApiConnectionDetails>();
            connectionDetails.Host = Host;
            connectionDetails.Port = Port;

            if (!string.IsNullOrWhiteSpace(BasicAuthUser))
            {
                var byteArray = Encoding.ASCII.GetBytes($"{BasicAuthUser}:{BasicAuthPassword.Value}");
                connectionDetails.AuthenticationHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }

            app.GetRequiredService<KongvergeWorkflowArguments>().FaultTolerance = FaultTolerance;
        }

        public Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            app.ShowHelp(false);
            return Task.FromResult(1);
        }
    }

    [Command("dry-run", Description = "Perform a dry-run synchronize of configuration on the target host from the specified folder (doesn't make any changes to the target host)")]
    public class DryRunCommand : ConvergeCommand
    {
        public override void Initialize(CommandLineApplication app)
        {
            base.Initialize(app);
            app.GetRequiredService<KongvergeWorkflowArguments>().DryRun = true;
        }
    }

    [Command("run", Description = "Synchronize configuration on the target host from the specified folder")]
    public class RunCommand : ConvergeCommand { }

    public abstract class ConvergeCommand : CommandBase
    {
        [Required]
        [DirectoryExists]
        [Argument(0, nameof(InputFolder), "Required. Folder for input data")]
        public string InputFolder { get; }

        public override void Initialize(CommandLineApplication app)
        {
            Parent.Initialize(app);
            app.GetRequiredService<KongvergeWorkflowArguments>().InputFolder = InputFolder;
        }

        public Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            Initialize(app);
            return app.GetRequiredService<KongvergeWorkflow>().Execute();
        }
    }

    [Command("export", Description = "Export existing configuration from the target host to the specified folder")]
    public class ExportCommand : CommandBase
    {
        [Required]
        [Argument(0, nameof(OutputFolder), "Required. Folder to output data from host")]
        public string OutputFolder { get; }

        public Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            Parent.Initialize(app);
            app.GetRequiredService<ExportWorkflowArguments>().OutputFolder = OutputFolder;
            return app.GetRequiredService<ExportWorkflow>().Execute();
        }
    }

    [HelpOption("-?|-h|--help")]
    public abstract class CommandBase
    {
        protected KongvergeCommand Parent { get; set; }

        public virtual void Initialize(CommandLineApplication app) { }
    }
}
