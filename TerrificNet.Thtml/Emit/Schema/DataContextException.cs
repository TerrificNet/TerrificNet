using System;
using System.Runtime.Serialization;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit.Schema
{
	[Serializable]
	internal class DataContextException : Exception
	{
		public SyntaxNode[] DependentNodes { get; }

		public DataContextException()
		{
		}

		public DataContextException(string message, SyntaxNode[] dependentNodes) : base(message)
		{
			DependentNodes = dependentNodes;
		}

		public DataContextException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected DataContextException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}