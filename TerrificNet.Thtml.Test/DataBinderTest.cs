using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TerrificNet.Thtml.Emit;
using Xunit;

namespace TerrificNet.Thtml.Test
{
    public class DataBinderTest
    {
        // ReSharper disable once UnusedMember.Global
        public static IEnumerable<object[]> BinderFactoriesParameter
        {
            get { return BinderFactories.Select(s => new object[] {s}); }
        }

        private static IEnumerable<Func<Type, IDataBinder>> BinderFactories
        {
            get
            {
                yield return type =>
                {
	                ParameterExpression dataContextParameter = Expression.Parameter(type);
	                return TypeDataScope.BinderFromType(dataContextParameter.Type);
                };
                yield return t => new DynamicDataScope();
            }
        }

		[Theory]
		[MemberData("BinderFactoriesParameter")]
		public void DataBinder_SimplePropertyToExpression(Func<Type, IDataBinder> dataBinderFactory)
		{
			const string expectedResult = "property";
			var obj = new { Property = expectedResult };

			var underTest = dataBinderFactory(obj.GetType());
			var result = underTest.Property("property");

			Assert.NotNull(result);
			var propertyResult = AssertExpression<string>(obj, result.BindString);

			Assert.Equal(expectedResult, propertyResult);
		}

		[Theory]
		[MemberData("BinderFactoriesParameter")]
		public void DataBinder_IterationPropertyToExpression(Func<Type, IDataBinder> dataBinderFactory)
		{
			const string expectedResult = "property";
			var obj = new { Property = new[] { new { Property2 = expectedResult } } };

			var underTest = dataBinderFactory(obj.GetType());
			var result = underTest.Property("property");

			Assert.NotNull(result);

			var childScope = result.Item();
			var propertyResult = AssertExpression<IEnumerable>(obj, b => result.BindEnumerable(b));

			Assert.NotNull(propertyResult);

			var innerPropertyResult = childScope.Property("property2");
			var innerPropertyEvaluator = CreateEval<string>(e => innerPropertyResult.BindString(e), childScope.DataContextType);

			foreach (var item in propertyResult)
			{
				var innerResult = innerPropertyEvaluator(item);
				Assert.Equal(expectedResult, innerResult);
			}
		}

		[Theory]
		[MemberData("BinderFactoriesParameter")]
		public void DataBinder_NestedPropertyToExpression(Func<Type, IDataBinder> dataBinderFactory)
		{
			const string expectedResult = "property";
			var obj = new { Property1 = new { Property2 = expectedResult } };

			var underTest = dataBinderFactory(obj.GetType());
			var result = underTest.Property("property1").Property("property2");

			Assert.NotNull(result);
			var propertyResult = AssertExpression<string>(obj, e => result.BindString(e));

			Assert.Equal(expectedResult, propertyResult);
		}

		[Theory]
		[MemberData("BinderFactoriesParameter")]
		public void DataBinder_ConditionalPropertyToExpression(Func<Type, IDataBinder> dataBinderFactory)
		{
			const bool expectedResult = true;
			var obj = new { Property1 = true };

			var underTest = dataBinderFactory(obj.GetType());
			var result = underTest.Property("property1");

			Assert.NotNull(result);
			var propertyResult = AssertExpression<bool>(obj, result.BindBoolean);

			Assert.Equal(expectedResult, propertyResult);
		}

		[Theory]
        [InlineData(typeof(List<string>), typeof(string))]
        [InlineData(typeof(IEnumerable<string>), typeof(string))]
        [InlineData(typeof(IDictionary<object, string>), typeof(KeyValuePair<object, string>))]
        public void TypeDataBinder_ItemFromGeneric(Type interfaceType, Type expectedItemType)
        {
	        ParameterExpression dataContextParameter = Expression.Parameter(interfaceType);
	        var underTest = TypeDataScope.BinderFromType(dataContextParameter.Type);
			var childScope = underTest.Item();

            Assert.NotNull(childScope);
            var binder = Assert.IsType<TypeDataScope>(childScope);

            Assert.Equal(expectedItemType, binder.DataContextType);
        }

		private static T AssertExpression<T>(object obj, Func<Expression, Expression> binding)
		{
			var type = obj.GetType();
			var eval = CreateEval<T>(binding, type);
			return eval(obj);
		}

	    private static Func<object, T> CreateEval<T>(Func<Expression, Expression> binding, Type type)
	    {
		    var dataContext = Expression.Parameter(typeof (object));
		    var evaluator = binding(Expression.Convert(dataContext, type));

		    var eval = Expression.Lambda<Func<object, T>>(evaluator, dataContext).Compile();
		    return eval;
	    }
    }
}
