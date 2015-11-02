using System;
using System.Collections.Generic;
using System.IO;

namespace TerrificNet.Thtml.Emit.Schema
{
	public class TypeScriptDefinitionVisitor : IDataSchemaVisitor
	{
		private readonly StringWriter _writer;
		private readonly IEnumerator<string> _nameGenerator;
		private readonly Stack<Tuple<string, ComplexDataSchema>> _types = new Stack<Tuple<string, ComplexDataSchema>>();
		private bool _inner;
		private string _typeName;

		public TypeScriptDefinitionVisitor(StringWriter writer)
		{
			_writer = writer;
			_nameGenerator = Names().GetEnumerator();
			_nameGenerator.MoveNext();
			_typeName = _nameGenerator.Current;
		}

		public void Visit(SimpleDataSchema simpleSchema)
		{
			Write(simpleSchema.Name.ToLower());
		}

		public void Visit(AnyDataSchema anyDataSchema)
		{
			Write("any");
		}

		public void Visit(ComplexDataSchema complexDataSchema)
		{
			HandleType(complexDataSchema, complexDataSchema1 =>
			{
				WriterHeader(_typeName);
				WriteProperties(complexDataSchema1);
			});
		}

		private void HandleType(ComplexDataSchema complexDataSchema, Action<ComplexDataSchema> proceed)
		{
			if (_inner)
			{
				_nameGenerator.MoveNext();
				var name = _nameGenerator.Current;

				_types.Push(new Tuple<string, ComplexDataSchema>(name, complexDataSchema));
				Write(name);
			}
			else
			{
				_inner = true;
				proceed(complexDataSchema);

				_inner = false;
				while (_types.Count > 0)
				{
					var type = _types.Pop();
					_typeName = type.Item1;
					type.Item2.Accept(this);
				}
			}
		}

		private void WriteProperties(ComplexDataSchema complexDataSchema)
		{
			Write(" { ");
			foreach (var property in complexDataSchema.Properties)
			{
				Write(property.Name);
				if (property.Schema != DataSchema.String && property.Schema.Nullable)
					Write("?");

				Write(":");
				property.Schema.Accept(this);
				Write("; ");
			}
			Write(" } ");
		}

		private void WriterHeader(string name)
		{
			_writer.Write("interface ");
			_writer.Write(name);
		}

		private void Write(string content)
		{
			_writer.Write(content);
		}

		public void Visit(IterableDataSchema iterableDataSchema)
		{
			HandleType(iterableDataSchema, cp =>
			{
				WriterHeader(_typeName);
				Write(" extends Array<");
				iterableDataSchema.ItemSchema.Accept(this);
				Write("> ");
				WriteProperties(iterableDataSchema);
			});
		}

		private IEnumerable<string> Names()
		{
			yield return "_global";
			int i = 0;
			while (true)
			{
				i++;
				yield return "t" + i;
			}
			// ReSharper disable once FunctionNeverReturns
		}
	}
}