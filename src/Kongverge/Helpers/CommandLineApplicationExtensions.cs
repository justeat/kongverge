using System;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;

namespace Kongverge.Helpers
{
    public static class CommandLineApplicationExtensions
    {
        public static int ExecuteWithErrorHandling(this CommandLineApplication app, string[] args)
        {
            try
            {
                return app.Execute(args);
            }
            catch (CommandParsingException e)
            {
                app.Error.WriteLine(e.Message);

                if (e is UnrecognizedCommandParsingException uex && uex.NearestMatches.Any())
                {
                    app.Error.WriteLine();
                    app.Error.WriteLine("Did you mean this?");
                    app.Error.WriteLine("    " + uex.NearestMatches.First());
                }

                return (int)ExitCode.UnspecifiedError;
            }
            catch (Exception e)
            {
                return ExitWithCode.Return(ExitCode.UnspecifiedError, e.ToString());
            }
        }
    }
}
