using System;
using System.Net;
using System.Runtime.Serialization;

namespace Kongverge.Services
{
    [Serializable]
    public class KongException : Exception
    {
        public KongException(HttpStatusCode statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }

        public KongException(HttpStatusCode statusCode, string message, Exception inner)
            : base(message, inner)
        {
            StatusCode = statusCode;
        }

        protected KongException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            StatusCode = (HttpStatusCode)info.GetInt32(nameof(StatusCode));
        }

        public HttpStatusCode StatusCode { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue(nameof(StatusCode), (int)StatusCode);
            base.GetObjectData(info, context);
        }
    }
}
