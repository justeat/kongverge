using System;
using System.IO;
using McMaster.Extensions.CommandLineUtils;

namespace Kongverge.IntegrationTests
{
    public class FakeConsole : IConsole
    {
        public FakeConsole()
        {
            CancelKeyPress = (sender, args) => { };
        }

        public void ResetColor() { }

        public TextWriter Out { get; set; }
        public TextWriter Error { get; set; }
        public TextReader In { get; set; }
        public bool IsInputRedirected { get; set; }
        public bool IsOutputRedirected { get; set; }
        public bool IsErrorRedirected { get; set; }
        public ConsoleColor ForegroundColor { get; set; }
        public ConsoleColor BackgroundColor { get; set; }
        public event ConsoleCancelEventHandler CancelKeyPress;
    }
}
