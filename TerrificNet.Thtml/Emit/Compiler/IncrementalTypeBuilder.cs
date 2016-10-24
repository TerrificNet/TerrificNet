using System;
using System.Reflection;
using System.Reflection.Emit;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class IncrementalTypeBuilder
	{
		public IncrementalTypeBuilder(Type type)
		{
			Type = type;
		}

		public IncrementalTypeBuilder AddField(Type type)
		{
			FieldInfo info;
			return AddField(type, out info);
		}

		public IncrementalTypeBuilder AddField(Type type, out FieldInfo field)
		{
			var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Test"), AssemblyBuilderAccess.Run);
			var moduleBuilder = assemblyBuilder.DefineDynamicModule("_dynamic");
			var typeBuilder = moduleBuilder.DefineType("Test", TypeAttributes.Class | TypeAttributes.Public, Type);
			var fieldBuilder = typeBuilder.DefineField("Gugus", type, FieldAttributes.Public);

			field = fieldBuilder;

			return new IncrementalTypeBuilder(typeBuilder.CreateTypeInfo().AsType());
		}

		public Type Type { get; }
	}
}
