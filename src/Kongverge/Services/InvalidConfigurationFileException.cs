using System;
using System.Runtime.Serialization;

namespace Kongverge.Services
{
    [Serializable]
    public class InvalidConfigurationFileException : Exception
    {
        public InvalidConfigurationFileException(string path, string message) : base(message)
        {
            Path = path;
        }

        public InvalidConfigurationFileException(string path, string message, Exception inner)
            : base(message, inner)
        {
            Path = path;
        }

        protected InvalidConfigurationFileException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Path = info.GetString(nameof(Path));
        }

        public string Path { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue(nameof(Path), Path);
            base.GetObjectData(info, context);
        }
    }
}
