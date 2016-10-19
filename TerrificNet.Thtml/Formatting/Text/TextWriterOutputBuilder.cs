using System;
using System.Collections.Generic;
using System.IO;

namespace TerrificNet.Thtml.Formatting.Text
{
	public class TextWriterOutputBuilder : IOutputBuilder
	{
		public TextWriterOutputBuilder(TextWriter adaptee)
		{
			Inner = adaptee;
		}

		internal TextWriter Inner { get; }

		public void ElementOpenStart(string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			Inner.Write("<");
			Inner.Write(tagName);

			if (staticProperties != null)
			{
				foreach (var prop in staticProperties)
				{
					Inner.Write(" ");
					Inner.Write(prop.Key);
					Inner.Write("=\"");
					Inner.Write(prop.Value);
					Inner.Write("\"");
				}
			}
		}

		public void ElementOpenEnd()
		{
			Inner.Write(">");
		}

		public void ElementOpen(string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			ElementOpenStart(tagName, staticProperties);
			ElementOpenEnd();
		}

		public void ElementClose(string tagName)
		{
			Inner.Write("</");
			Inner.Write(tagName);
			Inner.Write(">");
		}

		public void PropertyStart(string propertyName)
		{
			Inner.Write(" ");
			Inner.Write(propertyName);
			Inner.Write("=\"");
		}

		public void PropertyEnd()
		{
			Inner.Write("\"");
		}

		public void Value(string value)
		{
			Inner.Write(value);
		}
	}
}
