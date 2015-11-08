using System;
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
        public void DataBinder_SimpleProperty(Func<Type, IDataBinder> dataBinderFactory)
        {
            const string expectedResult = "property";
            var obj = new { Property = expectedResult };
            
            var underTest = dataBinderFactory(obj.GetType());
            var result = underTest.Property("property");

            Assert.NotNull(result);
            var evaluator = result.BindString();
            var propertyResult = evaluator.Evaluate(obj);

            Assert.Equal(expectedResult, propertyResult);
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
			var propertyResult = AssertExpression<string>(obj, result.BindStringToExpression);

			Assert.Equal(expectedResult, propertyResult);
		}

	    private static T AssertExpression<T>(object obj, Func<Expression, Expression> binding)
	    {
		    var dataContext = Expression.Parameter(typeof (object));
		    var evaluator = binding(Expression.Convert(dataContext, obj.GetType()));
		    Assert.Equal(typeof (T), evaluator.Type);

		    var eval = Expression.Lambda<Func<object, T>>(evaluator, dataContext).Compile();
		    var propertyResult = eval(obj);
		    return propertyResult;
	    }


	    [Theory]
        [MemberData("BinderFactoriesParameter")]
        public void DataBinder_IterationProperty(Func<Type, IDataBinder> dataBinderFactory)
        {
            const string expectedResult = "property";
            var obj = new { Property = new [] { new { Property2 = expectedResult } } };

            var underTest = dataBinderFactory(obj.GetType());
            var result = underTest.Property("property");

            Assert.NotNull(result);

	        IDataBinder childScope;
            var evaluator = result.BindEnumerable(out childScope);
            var propertyResult = evaluator.Evaluate(obj);

            Assert.NotNull(propertyResult);

            var itemResult = childScope;
            var innerPropertyResult = itemResult.Property("property2");

            var innerPropertyEvaluator = innerPropertyResult.BindString();

            foreach (var item in propertyResult)
            {
                var innerResult = innerPropertyEvaluator.Evaluate(item);
                Assert.Equal(expectedResult, innerResult);
            }
        }

        [Theory]
        [MemberData("BinderFactoriesParameter")]
        public void DataBinder_NestedProperty(Func<Type, IDataBinder> dataBinderFactory)
        {
            const string expectedResult = "property";
            var obj = new { Property1 = new { Property2 = expectedResult } };

            var underTest = dataBinderFactory(obj.GetType());
            var result = underTest.Property("property1").Property("property2");

            Assert.NotNull(result);
            var evaluator = result.BindString();
            var propertyResult = evaluator.Evaluate(obj);

            Assert.Equal(expectedResult, propertyResult);
        }

        [Theory]
        [MemberData("BinderFactoriesParameter")]
        public void DataBinder_ConditionalProperty(Func<Type, IDataBinder> dataBinderFactory)
        {
            const bool expectedResult = true;
            var obj = new { Property1 = true };

            var underTest = dataBinderFactory(obj.GetType());
            var result = underTest.Property("property1");

            Assert.NotNull(result);
            var evaluator = result.BindBoolean();
            var propertyResult = evaluator.Evaluate(obj);

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
			var propertyResult = AssertExpression<bool>(obj, result.BindBooleanToExpression);

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
	        IDataBinder childScope;
	        underTest.BindEnumerable(out childScope);

            Assert.NotNull(childScope);
            var binder = Assert.IsType<TypeDataScope>(childScope);

            Assert.Equal(expectedItemType, binder.DataContextType);
        }
    }
}
