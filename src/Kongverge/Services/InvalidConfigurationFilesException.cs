using System;
namespace Kongverge.Services
{
    [Serializable]
    public class InvalidConfigurationFilesException : Exception
    {
        public InvalidConfigurationFilesException(string message) : base(message) { }

        public InvalidConfigurationFilesException(string message, Exception inner) : base(message, inner) { }
    }
}
