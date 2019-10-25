using System;
using Serilog;

namespace Kongverge.Helpers
{
    public enum ExitCode
    {
        Success = 0,
        UnspecifiedError = 1,
        HostUnreachable = 2,
        InputFolderUnreachable = 3,
        InvalidConfigurationFiles = 4,
        HostVersionNotSupported = 5
    }

    public class ExitWithCode
    {
        public static int Return(ExitCode exitCode, string message = null)
        {
            switch (exitCode)
            {
                case ExitCode.Success:
                    Log.Information(message ?? "************** Finished **************");
                    break;

                case ExitCode.UnspecifiedError:
                    Log.Error(message ?? "Unspecified error");
                    break;

                case ExitCode.HostUnreachable:
                    Log.Error(message ?? "Specified host unreachable");
                    break;

                case ExitCode.InputFolderUnreachable:
                    Log.Error(message ?? "Unable to access input folder");
                    break;

                case ExitCode.InvalidConfigurationFiles:
                    Log.Error(message ?? "Invalid configuration file(s)");
                    break;

                case ExitCode.HostVersionNotSupported:
                    Log.Error(message ?? "Specified host's version is not supported");
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(exitCode), exitCode, null);
            }

            return (int)exitCode;
        }
    }
}
