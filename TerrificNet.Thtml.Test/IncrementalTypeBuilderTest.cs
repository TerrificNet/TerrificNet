using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TerrificNet.Thtml.Emit.Compiler;
using Xunit;

namespace TerrificNet.Thtml.Test
{
	public class IncrementalTypeBuilderTest
	{
		[Fact]
		public void IncrementalTypeBuilder_AddField_TypeContainsField()
		{
			var underTest = new IncrementalTypeBuilder(typeof(object));
			var resultBuilder = underTest.AddField(typeof(string));

			Assert.NotNull(resultBuilder.Type);
			var fields = resultBuilder.Type.GetTypeInfo().GetFields().ToList();
			Assert.Equal(1, fields.Count);
			Assert.Equal(typeof(string), fields[0].FieldType);
		}

		[Fact]
		public void IncrementalTypeBuilder_AddField_CanBeUsedInExpression()
		{
			var underTest = new IncrementalTypeBuilder(typeof(object));
			FieldInfo fieldInfo;
			var resultBuilder = underTest.AddField(typeof(string), out fieldInfo);

			var parameter = Expression.Parameter(resultBuilder.Type);
			var expression = Expression.Field(parameter, fieldInfo);
			var lambda = Expression.Lambda(expression, parameter);

			Assert.Equal(typeof(string), expression.Type);

			lambda.Compile();

		}

	}
}
