using System;
using System.Runtime.Serialization;

namespace Kongverge.Services
{
    [Serializable]
    public class InvalidConfigurationFilesException : Exception
    {
        public InvalidConfigurationFilesException(string message) : base(message) { }

        public InvalidConfigurationFilesException(string message, Exception inner) : base(message, inner) { }

        protected InvalidConfigurationFilesException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
