using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;

namespace Kongverge.DTOs
{
    public struct Password
    {
        public string Value { get; set; }

        public static void RegisterValueParser(CommandLineApplication app, IConsole console)
        {
            app.ValueParsers.Add(ValueParser.Create((name, value, culture) =>
            {
                if (string.IsNullOrWhiteSpace(value) && console.IsInputRedirected)
                {
                    value = console.In.ReadLine();
                }
                return new Password { Value = value };
            }));
        }
    }
}
