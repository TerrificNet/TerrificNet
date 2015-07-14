using System;
using System.Runtime.Serialization;

namespace TerrificNet.UnityModules
{
    [Serializable]
    public class InvalidApplicationException : Exception
    {
        public InvalidApplicationException()
        {
        }

        public InvalidApplicationException(string message)
            : base(message)
        {
        }

        public InvalidApplicationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected InvalidApplicationException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}