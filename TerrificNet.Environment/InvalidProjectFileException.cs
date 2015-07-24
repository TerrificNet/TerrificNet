using System;
using System.Runtime.Serialization;

namespace TerrificNet.Environment
{
    [Serializable]
    public class InvalidProjectFileException : Exception
    {
        public InvalidProjectFileException()
        {
        }

        public InvalidProjectFileException(string message) : base(message)
        {
        }

        public InvalidProjectFileException(string message, Exception inner) : base(message, inner)
        {
        }

        protected InvalidProjectFileException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}