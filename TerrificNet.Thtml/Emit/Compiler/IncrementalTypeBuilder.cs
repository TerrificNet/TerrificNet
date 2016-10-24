using System;
using System.Reflection;
using System.Reflection.Emit;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class IncrementalTypeBuilder
	{
		private readonly TypeBuilder _typeBuilder;

		private IncrementalTypeBuilder _defererredBuilder;

		private int _fieldCount;

		public IncrementalTypeBuilder(Type type) : this()
		{
			Type = type;
		}

		private IncrementalTypeBuilder()
		{
			var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Test"), AssemblyBuilderAccess.Run);
			var moduleBuilder = assemblyBuilder.DefineDynamicModule("_dynamic");
			_typeBuilder = moduleBuilder.DefineType("Test", TypeAttributes.Class | TypeAttributes.Public, Type);
		}

		public IncrementalTypeBuilder AddFieldAndCreate(Type type)
		{
			FieldInfoReference info;
			return AddFieldAndCreate(type, out info);
		}

		public IncrementalTypeBuilder AddFieldAndCreate(Type type, out FieldInfoReference field)
		{
			var fieldBuilder = AddFieldInternal(type);

			var resultType = _typeBuilder.CreateTypeInfo().AsType();
			var resultBuilder = new IncrementalTypeBuilder(resultType);
			field = new FieldInfoReference(resultBuilder, fieldBuilder);

			return resultBuilder;
		}

		private FieldBuilder AddFieldInternal(Type type)
		{
			_fieldCount++;
			return _typeBuilder.DefineField("F" + _fieldCount, type, FieldAttributes.Public);
		}

		public FieldInfoReference AddField(Type type)
		{
			var fieldBuilder = AddFieldInternal(type);
			return new FieldInfoReference(DeferredTypeBuilder, fieldBuilder);
		}

		public IncrementalTypeBuilder Complete()
		{
			DeferredTypeBuilder.Type = _typeBuilder.CreateTypeInfo().AsType();
			return _defererredBuilder;
		}

		private IncrementalTypeBuilder DeferredTypeBuilder
		{
			get
			{
				if (_defererredBuilder == null)
					_defererredBuilder = new IncrementalTypeBuilder();

				return _defererredBuilder;
			}
		}

		public Type Type { get; private set; }
	}

	public class FieldInfoReference
	{
		private readonly IncrementalTypeBuilder _builder;
		private readonly FieldBuilder _fieldBuilder;
		private FieldInfo _runtimeField;

		public FieldInfoReference(IncrementalTypeBuilder builder, FieldBuilder fieldBuilder)
		{
			_builder = builder;
			_fieldBuilder = fieldBuilder;
		}

		public FieldInfo FieldInfo
		{
			get
			{
				if (_runtimeField == null)
					_runtimeField = _builder.Type.GetRuntimeField(_fieldBuilder.Name);

				return _runtimeField;
			}
		}
	}
}
