using System;
using System.Collections.Generic;
using System.IO;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class TextWriterOutput : IOutputBuilder
	{
		private readonly TextWriter _adaptee;

		public TextWriterOutput(TextWriter adaptee)
		{
			_adaptee = adaptee;
			Inner = adaptee;
		}

		internal TextWriter Inner { get; }

		public void ElementOpenStart(string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			throw new NotImplementedException();
		}

		public void ElementOpenEnd()
		{
			throw new NotImplementedException();
		}

		public void ElementOpen(string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			throw new NotImplementedException();
		}

		public void ElementClose(string tagName)
		{
			throw new NotImplementedException();
		}

		public void PropertyStart(string propertyName)
		{
			throw new NotImplementedException();
		}

		public void PropertyEnd()
		{
			throw new NotImplementedException();
		}

		public void Value(string value)
		{
			throw new NotImplementedException();
		}
	}
}
