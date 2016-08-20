using System;
using System.Runtime.Serialization;

namespace TerrificNet.Environment
{
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
    }
}