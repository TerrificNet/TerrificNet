using System;
using System.Collections.Generic;
using System.IO;

namespace TerrificNet.Thtml.Rendering
{
	public class JavascriptIncrementalDomRenderer : IIncrementalDomRenderer, IDisposable
	{
		private readonly TextWriter _output;
		private readonly JavascriptMethodMapping _mapping;

		public JavascriptIncrementalDomRenderer(TextWriter output, JavascriptMethodMapping mapping)
		{
			_output = output;
			_mapping = mapping;
		}

		~JavascriptIncrementalDomRenderer()
		{
			Dispose(false);
		}

		public void ElementVoid(string tagName, string key, Dictionary<string, string> staticPropertyValuePairs, Dictionary<string, string> propertyValuePairs)
		{
			Element(_mapping.ElementVoid, tagName, key, staticPropertyValuePairs, propertyValuePairs);
		}

		public void ElementOpen(string tagName, string key, Dictionary<string, string> staticPropertyValuePairs, Dictionary<string, string> propertyValuePairs)
		{
			Element(_mapping.ElementOpen, tagName, key, staticPropertyValuePairs, propertyValuePairs);
		}

		public void ElementOpenStart(string tagName, string key, Dictionary<string, string> staticPropertyValuePairs, Dictionary<string, string> propertyValuePairs)
		{
			Element(_mapping.ElementOpenStart, tagName, key, staticPropertyValuePairs, propertyValuePairs);
		}

		public void Attr(string name, string value)
		{
			_output.Write(_mapping.Attr);
			_output.Write("(\"");
			_output.Write(name);
			_output.Write("\",\"");
			_output.Write(value);
			_output.Write("\"");
			_output.Write(");");
		}

		public void Text(string content)
		{
			_output.Write(_mapping.Text);
			_output.Write("(\"");
			WriteEncoded(content);
			_output.Write("\");");
		}

		private void WriteEncoded(string content)
		{
			foreach (var c in content)
			{
				switch (c)
				{
					case '\n':
						_output.Write("\\n");
						break;
					case '\r':
						_output.Write("\\r");
						break;
					case '\t':
						_output.Write("\\t");
						break;
					default:
						_output.Write(c);
						break;
				}
			}
		}

		public void ElementOpenEnd()
		{
			_output.Write(_mapping.ElementOpenEnd);
		}

		public void ElementClose(string tagName)
		{
			_output.Write(_mapping.ElementClose);
			_output.Write("(\"");
			_output.Write(tagName);
			_output.Write("\");");
		}

		private void Element(string methodName, string tagName, string key, Dictionary<string, string> staticPropertyValuePairs, Dictionary<string, string> propertyValuePairs)
		{
			_output.Write(methodName);
			_output.Write("(\"");
			_output.Write(tagName);
			_output.Write("\",\"");
			_output.Write(key);
			_output.Write("\",");
			if (staticPropertyValuePairs != null)
			{
				_output.Write("[");
				Attributes(_output, staticPropertyValuePairs);
				_output.Write("]");
			}
			else
				_output.Write("null");

			if (propertyValuePairs != null)
			{
				_output.Write(",");
				Attributes(_output, propertyValuePairs);
			}

			_output.Write(");");
		}

		private void Attributes(TextWriter output, Dictionary<string, string> staticPropertyValuePairs)
		{
			foreach (var attr in staticPropertyValuePairs)
			{
				output.Write("\"");
				output.Write(attr.Key);
				output.Write("\",\"");
				output.Write(attr.Value);
				output.Write("\"");
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_output.Dispose();
			}
		}
	}
}
