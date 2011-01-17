using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Flubu.Tasks.Virtual.HyperV
{
    [Serializable]
    public class VirtualServerException : Exception
    {
        public VirtualServerException()
        {
        }

        public VirtualServerException(string message)
            : base(message)
        {
        }

        public VirtualServerException(string errorCode, string message)
            : base(string.Format(CultureInfo.InvariantCulture, "{0};{1}", errorCode, message))
        {
        }

        public VirtualServerException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected VirtualServerException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}