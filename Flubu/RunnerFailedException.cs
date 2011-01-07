using System;
using System.Globalization;

namespace Flubu
{
    [Serializable]
    public class RunnerFailedException : Exception
    {
        public RunnerFailedException ()
        {
        }

        public RunnerFailedException (string message) : base (message)
        {
        }

        public RunnerFailedException(string formatMessage, object[] arguments)
            : base(string.Format(CultureInfo.InvariantCulture, formatMessage, arguments))
        {
        }

        public RunnerFailedException(string formatMessage, object first)
            : base(string.Format(CultureInfo.InvariantCulture, formatMessage, first))
        {
        }

        public RunnerFailedException (string message, Exception innerException) : base (message, innerException)
        {
        }

        protected RunnerFailedException (
            System.Runtime.Serialization.SerializationInfo serializationInfo,
            System.Runtime.Serialization.StreamingContext context)
            : base (serializationInfo, context)
        {
        }
    }
}
