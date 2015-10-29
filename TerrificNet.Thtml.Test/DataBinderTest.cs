using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        private static IEnumerable<Func<Type, IDataScope>> BinderFactories
        {
            get
            {
                yield return TypeDataScope.BinderFromType;
                yield return t => new DynamicDataScope();
            }
        }

        [Theory]
        [MemberData("BinderFactoriesParameter")]
        public void DataBinder_SimpleProperty(Func<Type, IDataScope> dataBinderFactory)
        {
            const string expectedResult = "property";
            var obj = new { Property = expectedResult };
            
            var underTest = dataBinderFactory(obj.GetType());
            var result = underTest.Property("property", null);

            Assert.NotNull(result);
            var evaluator = result.BindString();
            var propertyResult = evaluator.Evaluate(new ObjectDataContext(obj));

            Assert.Equal(expectedResult, propertyResult);
        }

        [Theory]
        [MemberData("BinderFactoriesParameter")]
        public void DataBinder_IterationProperty(Func<Type, IDataScope> dataBinderFactory)
        {
            const string expectedResult = "property";
            var obj = new { Property = new [] { new { Property2 = expectedResult } } };

            var underTest = dataBinderFactory(obj.GetType());
            var result = underTest.Property("property", null);

            Assert.NotNull(result);

	        IDataScope childScope;
            var evaluator = result.BindEnumerable(out childScope);
            var propertyResult = evaluator.Evaluate(new ObjectDataContext(obj));

            Assert.NotNull(propertyResult);

            var itemResult = childScope;
            var innerPropertyResult = itemResult.Property("property2", null);

            var innerPropertyEvaluator = innerPropertyResult.BindString();

            foreach (var item in propertyResult)
            {
                var innerResult = innerPropertyEvaluator.Evaluate(new ObjectDataContext(item));
                Assert.Equal(expectedResult, innerResult);
            }
        }

        [Theory]
        [MemberData("BinderFactoriesParameter")]
        public void DataBinder_NestedProperty(Func<Type, IDataScope> dataBinderFactory)
        {
            const string expectedResult = "property";
            var obj = new { Property1 = new { Property2 = expectedResult } };

            var underTest = dataBinderFactory(obj.GetType());
            var result = underTest.Property("property1", null).Property("property2", null);

            Assert.NotNull(result);
            var evaluator = result.BindString();
            var propertyResult = evaluator.Evaluate(new ObjectDataContext(obj));

            Assert.Equal(expectedResult, propertyResult);
        }

        [Theory]
        [MemberData("BinderFactoriesParameter")]
        public void DataBinder_ConditionalProperty(Func<Type, IDataScope> dataBinderFactory)
        {
            const bool expectedResult = true;
            var obj = new { Property1 = true };

            var underTest = dataBinderFactory(obj.GetType());
            var result = underTest.Property("property1", null);

            Assert.NotNull(result);
            var evaluator = result.BindBoolean();
            var propertyResult = evaluator.Evaluate(new ObjectDataContext(obj));

            Assert.Equal(expectedResult, propertyResult);
        }

        [Theory]
        [InlineData(typeof(List<string>), typeof(string))]
        [InlineData(typeof(IEnumerable<string>), typeof(string))]
        [InlineData(typeof(IDictionary<object, string>), typeof(KeyValuePair<object, string>))]
        public void TypeDataBinder_ItemFromGeneric(Type interfaceType, Type expectedItemType)
        {
            var underTest = TypeDataScope.BinderFromType(interfaceType);
	        IDataScope childScope;
	        underTest.BindEnumerable(out childScope);

            Assert.NotNull(childScope);
            var binder = Assert.IsType<TypeDataScope>(childScope);

            Assert.Equal(expectedItemType, binder.ResultType);
        }
    }
}
