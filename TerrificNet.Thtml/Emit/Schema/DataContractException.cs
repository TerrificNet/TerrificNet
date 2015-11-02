using System;
using System.Runtime.Serialization;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit.Schema
{
	[Serializable]
	internal class DataContractException : Exception
	{
		public SyntaxNode[] DependentNodes { get; }

		public DataContractException()
		{
		}

		public DataContractException(string message, SyntaxNode[] dependentNodes) : base(message)
		{
			DependentNodes = dependentNodes;
		}

		public DataContractException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected DataContractException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}