using System;
using Serilog;

namespace Kongverge.Helpers
{
    public enum ExitCode
    {
        Success = 0,
        HostUnreachable,
        InvalidPort,
        MissingHost,
        MissingPort,
        InputFolderUnreachable,
        IncompatibleArguments,
        InvalidConfigurationFile,
        UnspecifiedError
    }

    public class ExitWithCode
    {
        public static int Return(ExitCode exitCode, string message = null)
        {
            switch (exitCode)
            {
                case ExitCode.Success:
                    Log.Information(message ?? "Finished");
                    break;

                case ExitCode.InvalidPort:
                    Log.Error(message ?? "Invalid port specified");
                    break;

                case ExitCode.MissingHost:
                    Log.Error(message ?? "Host must be specified");
                    break;

                case ExitCode.MissingPort:
                    Log.Error(message ?? "Port must be specified");
                    break;

                case ExitCode.HostUnreachable:
                    Log.Error(message ?? "Specified host unreachable");
                    break;

                case ExitCode.InputFolderUnreachable:
                    Log.Error(message ?? "Unable to access input folder");
                    break;

                case ExitCode.IncompatibleArguments:
                    Log.Error(message ?? "Incompatible command line arguments");
                    break;

                case ExitCode.InvalidConfigurationFile:
                    Log.Error(message ?? "Invalid configuration file");
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(exitCode), exitCode, null);
            }

            return (int)exitCode;
        }
    }
}
